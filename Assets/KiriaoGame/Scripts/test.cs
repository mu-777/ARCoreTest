using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour {
    // Start is called before the first frame update
    IEnumerator Start() {
        while (true) {
            var op = new AsyncOperation();
            print("aaa");
            System.Threading.Thread.Sleep(1000);
            yield return op;
        }
    }

}
