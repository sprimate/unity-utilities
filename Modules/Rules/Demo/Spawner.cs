using UnityEngine;

namespace HitTrax.ErrorHandling
{
    public class Spawner : MonoBehaviour
    {
        public float timeBetweenSpawn;
        public GameObject spawnObject;
        public float minVel;
        public float maxVel;
        private float _timeSinceSpawn;

        private void Start()
        {
            Spawn();
        }

        // Update is called once per frame
        void Update()
        {
            _timeSinceSpawn += Time.deltaTime;

            if (_timeSinceSpawn >= timeBetweenSpawn)
            {
                Spawn();
            }

        }

        void Spawn()
        {
            _timeSinceSpawn = 0f;
            var obj = GameObject.Instantiate(spawnObject, this.transform.position, Quaternion.identity);
            obj.SetActive(true);
            obj.transform.position = this.transform.position;
            var rb = obj.AddComponent<Rigidbody>();            
            rb.linearVelocity = new Vector3(0, 0, Vel());
        }

        float Vel() => Random.Range(minVel, maxVel);


    }
}
