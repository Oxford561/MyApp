using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
    public partial class Sys_SceneDetailDBModel
    {
        private List<Sys_SceneDetailEntity> m_retList = new List<Sys_SceneDetailEntity>();

        public List<Sys_SceneDetailEntity> GetListBySceneId(int sceneId, int sceneGrade)
        {
            m_retList.Clear();
            List<Sys_SceneDetailEntity> lst = this.GetList();
            int len = lst.Count;
            for (int i = 0; i < len; i++)
            {
                Sys_SceneDetailEntity entity = lst[i];
                if (entity.SceneId == sceneId && entity.SceneGrade <= sceneGrade)
                {
                    m_retList.Add(entity);
                }
            }
            return m_retList;
        }
    }
}