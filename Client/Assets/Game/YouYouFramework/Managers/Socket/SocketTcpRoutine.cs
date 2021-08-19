using Google.Protobuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;


namespace YouYou
{
	/// <summary>
	/// SocketTcp������
	/// </summary>
	public class SocketTcpRoutine
	{
		#region ������Ϣ�������
		//������Ϣ����
		private Queue<byte[]> m_SendQueue = new Queue<byte[]>();

		//ѹ������ĳ��Ƚ���
		private const int m_CompressLen = 200;

		/// <summary>
		/// ��ǰ֡��������
		/// </summary>
		private int m_SendCount = 0;

		/// <summary>
		/// �Ƿ���δ������ֽ�
		/// </summary>
		private bool m_IsHasUnDealBytes = false;
		/// <summary>
		/// δ������ֽ�
		/// </summary>
		private byte[] m_UnDealBytes = null;
		#endregion

		#region ������Ϣ�������
		//�������ݰ��Ļ���������
		private MMO_MemoryStream m_ReceiveMS = new MMO_MemoryStream();
		//�������ݰ����ֽ����黺����
		private byte[] m_ReceiveBuffer = new byte[1024];

		//������Ϣ�Ķ���
		private Queue<byte[]> m_ReceiveQueue = new Queue<byte[]>();

		/// <summary>
		/// ��ǰ֡��������
		/// </summary>
		private int m_ReceiveCount = 0;
		#endregion

		/// <summary>
		/// �������ݵ�MemoryStream
		/// </summary>
		private MMO_MemoryStream m_SocketSendMS = new MMO_MemoryStream();
		/// <summary>
		/// �������ݵ�MemoryStream
		/// </summary>
		private MMO_MemoryStream m_SocketReceiveMS = new MMO_MemoryStream();

		/// <summary>
		/// �ͻ���socket
		/// </summary>
		private Socket m_Client;

		/// <summary>
		/// ���ӳɹ�ʱ����һ��
		/// </summary>
		public Action OnConnectOK;

		internal void OnUpdate()
		{
			#region �Ӷ����л�ȡ����
			while (true)
			{
				if (m_ReceiveCount <= GameEntry.Socket.MaxReceiveCount)
				{
					m_ReceiveCount++;
					lock (m_ReceiveQueue)
					{
						if (m_ReceiveQueue.Count > 0)
						{
							//�õ������е����ݰ�
							byte[] buffer = m_ReceiveQueue.Dequeue();

							//���֮�������
							byte[] bufferNew = new byte[buffer.Length - 1];

							bool isCompress = false;

							//1.���������е����ݰ�
							MMO_MemoryStream ms1 = m_SocketReceiveMS;
							ms1.SetLength(0);
							ms1.Write(buffer, 0, buffer.Length);
							ms1.Position = 0;

							isCompress = ms1.ReadBool();
							ms1.Read(bufferNew, 0, bufferNew.Length);

							if (isCompress)
							{
								bufferNew = ZlibHelper.DeCompressBytes(bufferNew);
							}

							ushort protoCode = 0;
							ProtoCategory protoCategory;
							byte[] protoContent = new byte[bufferNew.Length - 3];//����-3 �Ǽ�ȥ protoCode����+protoCategory����

							MMO_MemoryStream ms2 = m_SocketReceiveMS;
							ms2.SetLength(0);
							ms2.Write(bufferNew, 0, bufferNew.Length);
							ms2.Position = 0;

							//Э����
							protoCode = ms2.ReadUShort();
							protoCategory = (ProtoCategory)ms2.ReadByte();

							ms2.Read(protoContent, 0, protoContent.Length);

							//��� �õ�ԭʼ����
							protoContent = SecurityUtil.Xor(protoContent);
							GameEntry.Event.SocketEvent.Dispatch(protoCode, protoContent);
						}
						else
						{
							break;
						}
					}
				}
				else
				{
					m_ReceiveCount = 0;
					break;
				}
			}
			#endregion

			CheckSendQueue();
		}


		#region Connect ���ӵ�socket������
		/// <summary>
		/// ���ӵ�socket������
		/// </summary>
		/// <param name="ip">ip</param>
		/// <param name="port">�˿ں�</param>
		public void Connect(string ip, int port)
		{
			//���socket�Ѿ����� ���Ҵ���������״̬ ��ֱ�ӷ���
			if (m_Client != null && m_Client.Connected) return;

			string newServerIp = ip;
			AddressFamily addressFamily = AddressFamily.InterNetwork;

#if UNITY_IPHONE && !UNITY_EDITOR && SDKCHANNEL_APPLE_STORE
        AppleStoreInterface.GetIPv6Type(ip, port.ToString(), out newServerIp, out addressFamily);
#endif

			m_Client = new Socket(addressFamily, SocketType.Stream, ProtocolType.Tcp);

			try
			{
				m_Client.BeginConnect(new IPEndPoint(IPAddress.Parse(newServerIp), port), ConnectCallBack, m_Client);

			}
			catch (Exception ex)
			{
				Debug.Log("����ʧ��=" + ex.Message);
			}
		}
		/// <summary>
		/// ��������ص�
		/// </summary>
		/// <param name="ar"></param>
		private void ConnectCallBack(IAsyncResult ar)
		{
			if (m_Client.Connected)
			{
				GameEntry.Socket.RegisterSocketTcpRoutine(this);

				ReceiveMsg();

				GameEntry.Log(LogCategory.Proto, "Socket���ӵ�=={0}==�������ɹ�!", m_Client.RemoteEndPoint);
				OnConnectOK?.Invoke();
			}
			else
			{
				Debug.Log("socket����ʧ��");
			}
			m_Client.EndConnect(ar);
		}
		#endregion

		#region DisConnect �ӵ�ǰ���ӵ�Socket�������Ͽ�
		/// <summary>
		/// �ӵ�ǰ���ӵ�Socket�������Ͽ�
		/// </summary>
		public void DisConnect()
		{
			if (m_Client != null && m_Client.Connected)
			{
				GameEntry.Log(LogCategory.Proto, "Socket��=={0}==�������Ͽ�����!==", m_Client.RemoteEndPoint);
				m_Client.Shutdown(SocketShutdown.Both);
				m_Client.Close();
				GameEntry.Socket.RemoveSocketTcpRoutine(this);
			}
		}
		#endregion

		#region CheckSendQueue ��鷢�Ͷ���
		/// <summary>
		/// ��鷢�Ͷ���
		/// </summary>
		private void CheckSendQueue()
		{
			if (m_SendCount >= GameEntry.Socket.MaxSendCount)
			{
				//�ȴ���һ֡����
				m_SendCount = 0;
				return;
			}

			lock (m_SendQueue)
			{
				if (m_SendQueue.Count > 0 || m_IsHasUnDealBytes)
				{
					MMO_MemoryStream ms = m_SocketSendMS;
					ms.SetLength(0);

					if (m_IsHasUnDealBytes)
					{
						m_IsHasUnDealBytes = false;
						ms.Write(m_UnDealBytes, 0, m_UnDealBytes.Length);
					}

					while (true)
					{
						if (m_SendQueue.Count == 0) break;

						//ȡ��һ���ֽ�����
						byte[] buffer = m_SendQueue.Dequeue();
						if (buffer.Length + ms.Length <= GameEntry.Socket.MaxSendByteCount)
						{
							ms.Write(buffer, 0, buffer.Length);
						}
						else
						{
							//�Ѿ�ȡ����һ��Ҫ���͵��ֽ�����
							m_UnDealBytes = buffer;
							m_IsHasUnDealBytes = true;
							break;
						}
					}

					m_SendCount++;
					Send(ms.ToArray());
				}
			}
		}
		#endregion

		#region MakeData ��װ���ݰ�
		/// <summary>
		/// ��װ���ݰ�
		/// </summary>
		/// <param name="proto"></param>
		/// <returns></returns>
		private byte[] MakeData(IProto proto)
		{
			byte[] retBuffer = null;

			byte[] data = proto.ToByteArray();
			//1.������ݰ��ĳ��� ������m_CompressLen �����ѹ��
			bool isCompress = data.Length > m_CompressLen ? true : false;
			if (isCompress)
			{
				data = ZlibHelper.CompressBytes(data);
			}

			//2.���
			data = SecurityUtil.Xor(data);

			MMO_MemoryStream ms = m_SocketSendMS;
			ms.SetLength(0);

			ms.WriteUShort((ushort)(data.Length + 4)); //4=isCompress 1 + ProtoId 2 + Category 1

			ms.WriteBool(isCompress);

			ms.WriteUShort(proto.ProtoId);
			ms.WriteByte((byte)proto.Category);

			ms.Write(data, 0, data.Length);

			retBuffer = ms.ToArray();
			return retBuffer;
		}

		/// <summary>
		/// ��װ���ݰ�
		/// </summary>
		/// <param name="protoId"></param>
		/// <param name="category"></param>
		/// <param name="buffer"></param>
		/// <returns></returns>
		private byte[] MakeData(ushort protoId, byte category, byte[] buffer)
		{
			byte[] retBuffer = null;

			byte[] data = buffer;
			//1.������ݰ��ĳ��� ������m_CompressLen �����ѹ��
			bool isCompress = data.Length > m_CompressLen ? true : false;
			if (isCompress)
			{
				data = ZlibHelper.CompressBytes(data);
			}

			//2.���
			data = SecurityUtil.Xor(data);

			MMO_MemoryStream ms = m_SocketSendMS;
			ms.SetLength(0);

			ms.WriteUShort((ushort)(data.Length + 4)); //4=isCompress 1 + ProtoId 2 + Category 1

			ms.WriteBool(isCompress);

			ms.WriteUShort(protoId);
			ms.WriteByte(category);

			ms.Write(data, 0, data.Length);

			retBuffer = ms.ToArray();
			return retBuffer;
		}
		#endregion

		#region SendMsg ������Ϣ ����Ϣ�������
		/// <summary>
		/// ������Ϣ
		/// </summary>
		/// <param name="buffer"></param>
		public void SendMsg(IProto proto)
		{
			//�õ���װ������ݰ�
			byte[] sendBuffer = MakeData(proto);

			lock (m_SendQueue)
			{
				//�����ݰ��������
				m_SendQueue.Enqueue(sendBuffer);
			}
		}

		public void SendMsg(ushort protoId, byte category, byte[] buffer)
		{
			//�õ���װ������ݰ�
			byte[] sendBuffer = MakeData(protoId, category, buffer);

			lock (m_SendQueue)
			{
				//�����ݰ��������
				m_SendQueue.Enqueue(sendBuffer);
			}
		}
		#endregion

		#region Send �����������ݰ���������
		/// <summary>
		/// �����������ݰ���������
		/// </summary>
		/// <param name="buffer"></param>
		private void Send(byte[] buffer)
		{
			m_Client.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, SendCallBack, m_Client);
		}
		#endregion

		#region SendCallBack �������ݰ��Ļص�
		/// <summary>
		/// �������ݰ��Ļص�
		/// </summary>
		/// <param name="ar"></param>
		private void SendCallBack(IAsyncResult ar)
		{
			m_Client.EndSend(ar);
		}
		#endregion

		//====================================================

		#region ReceiveMsg ��������
		/// <summary>
		/// ��������
		/// </summary>
		private void ReceiveMsg()
		{
			//�첽��������
			m_Client.BeginReceive(m_ReceiveBuffer, 0, m_ReceiveBuffer.Length, SocketFlags.None, ReceiveCallBack, m_Client);
		}
		#endregion

		#region IsSocketConnected �ж�socket�Ƿ�����
		/// <summary>
		/// �ж�socket�Ƿ�����
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		bool IsSocketConnected(Socket s)
		{
			return !((s.Poll(1000, SelectMode.SelectRead) && (s.Available == 0)) || !s.Connected);
		}
		#endregion

		#region ReceiveCallBack �������ݻص�
		/// <summary>
		/// �������ݻص�
		/// </summary>
		/// <param name="ar"></param>
		private void ReceiveCallBack(IAsyncResult ar)
		{
			try
			{
				if (IsSocketConnected(m_Client))
				{
					int len = m_Client.EndReceive(ar);

					if (len > 0)
					{
						//�Ѿ����յ�����

						//�ѽ��յ����� д�뻺����������β��
						m_ReceiveMS.Position = m_ReceiveMS.Length;
						//��ָ�����ȵ��ֽ� д��������
						m_ReceiveMS.Write(m_ReceiveBuffer, 0, len);
						//��������������ĳ���>2 ˵�������и��������İ�������
						//Ϊʲô������2 ��Ϊ���ǿͻ��˷�װ���ݰ� �õ�ushort ���Ⱦ���2
						if (m_ReceiveMS.Length > 2)
						{
							//����ѭ�� ������ݰ�
							while (true)
							{
								//��������ָ��λ�÷���0��
								m_ReceiveMS.Position = 0;

								//currMsgLen = ����ĳ���
								int currMsgLen = m_ReceiveMS.ReadUShort();

								//currFullMsgLen �ܰ��ĳ���=��ͷ����+���峤��
								int currFullMsgLen = 2 + currMsgLen;

								//����������ĳ���>=�����ĳ��� ˵�������յ���һ��������
								if (m_ReceiveMS.Length >= currFullMsgLen)
								{
									//�����յ�һ��������

									//��������byte[]����
									byte[] buffer = new byte[currMsgLen];

									//��������ָ��ŵ�2��λ�� Ҳ���ǰ����λ��
									m_ReceiveMS.Position = 2;

									//�Ѱ������byte[]����
									m_ReceiveMS.Read(buffer, 0, currMsgLen);

									lock (m_ReceiveQueue)
									{
										m_ReceiveQueue.Enqueue(buffer);
									}
									//==============����ʣ���ֽ�����===================

									//ʣ���ֽڳ���
									int remainLen = (int)m_ReceiveMS.Length - currFullMsgLen;
									if (remainLen > 0)
									{
										//��ָ����ڵ�һ������β��
										m_ReceiveMS.Position = currFullMsgLen;

										//����ʣ���ֽ�����
										byte[] remainBuffer = new byte[remainLen];

										//������������ʣ���ֽ�����
										m_ReceiveMS.Read(remainBuffer, 0, remainLen);

										//���������
										m_ReceiveMS.Position = 0;
										m_ReceiveMS.SetLength(0);

										//��ʣ���ֽ���������д��������
										m_ReceiveMS.Write(remainBuffer, 0, remainBuffer.Length);

										remainBuffer = null;
									}
									else
									{
										//û��ʣ���ֽ�

										//���������
										m_ReceiveMS.Position = 0;
										m_ReceiveMS.SetLength(0);

										break;
									}
								}
								else
								{
									//��û���յ�������
									break;
								}
							}
						}
					}

					//������һ�ν������ݰ�
					ReceiveMsg();
				}
				else
				{
					//�������Ͽ�����
					Debug.Log(string.Format("������{0}�Ͽ�����", m_Client.RemoteEndPoint.ToString()));
				}
			}
			catch
			{
				//�������Ͽ�����
				Debug.Log(string.Format("������{0}�Ͽ�����", m_Client.RemoteEndPoint.ToString()));
			}
		}
		#endregion
	}
}