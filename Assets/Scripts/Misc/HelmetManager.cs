using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
namespace HelmetManage
{
    public class HelmetManager : NetworkBehaviour
    {
        public static HelmetManager instance;
        public GameObject HelmetPrefab;
        public GameObject[] Ignore;
        List<Collider2D> collidersIgnore = new List<Collider2D>();
        private void Awake()
        {
            foreach (var i in Ignore)
                foreach (var c in i.GetComponents<Collider2D>())
                {
                    collidersIgnore.Add(c);
                }
            instance = this;
        }
        public override void OnStartServer()
        {
            SpawnHelmet();
        }
        [Server]
        public void SpawnHelmet()
        {
            foreach (var player in SceneScript.instance.players.Values)
            {
                //if(FindObjectOfType<Helmet>() == null)
                //{
                //    return;
                //}
                if (player.HasHelmet)
                {
#if UNITY_EDITOR
                    Debug.Log($"player {player.netId} has helmet");
#endif
                    return;
                }
            }
            var hel = Instantiate(HelmetPrefab, transform.position, Quaternion.identity);
            foreach (var c in collidersIgnore)
            {
                Physics2D.IgnoreCollision(c, hel.GetComponentInChildren<Collider2D>(), true);
            }
            NetworkServer.Spawn(hel);
        }
    }
}