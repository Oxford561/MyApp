//===================================================
//作    者：边涯  http://www.u3dol.com  QQ群：87481002
//创建时间：2015-12-01 21:45:22
//备    注：
//===================================================
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using YouYou;
using DG.Tweening;

/// <summary>
/// GameObject拓展类
/// </summary>
public static class GameObjectUtil
{
    #region GetOrCreatComponent 获取或创建组件
    /// <summary>
    /// 获取或创建组件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static T GetOrCreatComponent<T>(this GameObject obj) where T : MonoBehaviour
    {
        T t = obj.GetComponent<T>();
        if (t == null)
        {
            t = obj.AddComponent<T>();
        }
        return t;
    }
    #endregion

    #region SetLayer 设置当前gameObject及所有子物体的层
    /// <summary>
    /// 设置当前gameObject及所有子物体的层
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="layerName">层的名称</param>
    public static void SetLayer(this GameObject obj, string layerName)
    {
        Transform[] transArr = obj.transform.GetComponentsInChildren<Transform>();
        for (int i = 0; i < transArr.Length; i++)
        {
            transArr[i].gameObject.layer = LayerMask.NameToLayer(layerName);
        }
    }
    #endregion

    #region SetParent 初始化当前gameObject并设置他的父物体
    /// <summary>
    /// 初始化当前gameObject并设置他的父物体
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="parent"></param>
    public static void SetParent(this GameObject obj, Transform parent)
    {
        Vector3 pos = obj.transform.localPosition;
        Vector3 scale = obj.transform.localScale;
        Vector3 eulerAngles = obj.transform.localEulerAngles;

        obj.transform.SetParent(parent);

        obj.transform.localPosition = pos;
        obj.transform.localScale = scale;
        obj.transform.localEulerAngles = eulerAngles;
    }
    #endregion

    #region DeepFindChild 深度递归查找子节点
    /// <summary>
    /// 深度递归查找子节点
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="targetName"></param>
    /// <returns></returns>
    public static Transform DeepFind(this Transform parent, string targetName)
    {
        Transform _result = null;
        _result = parent.Find(targetName);
        if (_result == null)
        {
            foreach (Transform child in parent)
            {
                _result = DeepFind(child, targetName);
                if (_result != null)
                {
                    return _result;
                }
            }
        }
        return _result;
    }
    #endregion

    #region 清空数组相关
    /// <summary>
    /// 清空数组
    /// </summary>
    /// <param name="arr"></param>
    public static void SetNull(this MonoBehaviour[] arr)
    {
        if (arr != null)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = null;
            }
            arr = null;
        }
    }
    /// <summary>
    /// 清空数组
    /// </summary>
    /// <param name="arr"></param>
    public static void SetNull(this Transform[] arr)
    {
        if (arr != null)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = null;
            }
            arr = null;
        }
    }
    /// <summary>
    /// 清空数组
    /// </summary>
    /// <param name="arr"></param>
    public static void SetNull(this Sprite[] arr)
    {
        if (arr != null)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = null;
            }
            arr = null;
        }
    }
    #endregion


    #region UI相关
    /// <summary>
    /// 设置Text值
    /// </summary>
    /// <param name="text">内容</param>
    public static void SetText(this Text txtObj, string text)
    {
        if (txtObj != null)
        {
            txtObj.text = text;
        }
    }
    public static void SetText(this Text txtObj, string text, bool isAnimation = false, ScrambleMode scramblMode = ScrambleMode.None)
    {
        if (txtObj != null)
        {
            if (isAnimation)
            {
                txtObj.text = "";
                txtObj.DOText(text, 0.5f, scrambleMode: scramblMode);
            }
            else
            {
                txtObj.text = text;
            }
        }
    }
    /// <summary>
    /// 设置滑动条Sliderd的值
    /// </summary>
    /// <param name="sliderObj"></param>
    /// <param name="value"></param>
    public static void SetSliderValue(this Slider sliderObj, float value)
    {
        if (sliderObj != null)
        {
            sliderObj.value = value;
        }
    }
    /// <summary>
    /// 设置Image的图片
    /// </summary>
    /// <param name="imgObj"></param>
    /// <param name="sprite"></param>
    public static void SetImage(this Image imgObj, Sprite sprite)
    {
        if (imgObj != null)
        {
            imgObj.sprite = sprite;
        }
    }
    #endregion
    #region AutoLoadTexture 自动加载图片
    /// <summary>
    /// 自动加载图片
    /// </summary>
    /// <param name="go"></param>
    /// <param name="imgPath"></param>
    /// <param name="imgName"></param>
    public static void AutoLoadTexture(this Image img, string imgPath, bool isSetNativeSize = false)
    {
        if (img != null && !string.IsNullOrEmpty(imgPath))
        {
            GameEntry.Resource.ResourceLoaderManager.LoadMainAsset( imgPath, (Object asset) =>
            {
                if (asset == null) return;

                Sprite obj = null;
                if (asset is Sprite)
                {
                    obj = (Sprite)asset;
                }
                else
                {
                    Texture2D texture = (Texture2D)asset;
                    obj = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                }

                img.overrideSprite = obj;
                if (isSetNativeSize)
                {
                    img.SetNativeSize();
                }
            });
        }
    }
    /// <summary>
    /// 自动加载图片
    /// </summary>
    /// <param name="go"></param>
    /// <param name="imgPath"></param>
    /// <param name="imgName"></param>
    public static void AutoLoadTexture(this RawImage img, string imgPath, bool isSetNativeSize = false)
    {
        if (img != null && !string.IsNullOrEmpty(imgPath))
        {
            GameEntry.Resource.ResourceLoaderManager.LoadMainAsset( "Assets/Download/UI/UIRes/UITexture/" + imgPath, (Object asset) =>
            {
                if (asset == null) return;

                if (asset is Texture2D) img.texture = (Texture2D)asset;
                if (isSetNativeSize) img.SetNativeSize();
            });
        }
    }
    #endregion
}