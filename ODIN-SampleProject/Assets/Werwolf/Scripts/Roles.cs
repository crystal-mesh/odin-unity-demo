using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Werwolf.Scripts
{
    [CreateAssetMenu(fileName = "Roles", menuName = "Werewolf/Roles", order = 0)]
    public class Roles : ScriptableObject
    {
        [FormerlySerializedAs("roles")] public List<RoleData> list = new List<RoleData>();

        /// <summary>
        ///     For converting string to enum
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public RoleTypes FromString(string name)
        {
            foreach (RoleData roleData in list)
                if (roleData.name == name)
                    return roleData.type;
            return RoleTypes.Undefined;
        }

        /// <summary>
        ///     For converting enum to string
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public string ToString(RoleTypes type)
        {
            foreach (var data in list)
                if (data.type == type)
                    return data.name;

            return null;
        }
    }

    [Serializable]
    public class RoleData
    {
        public RoleTypes type;
        public string name;
    }
}