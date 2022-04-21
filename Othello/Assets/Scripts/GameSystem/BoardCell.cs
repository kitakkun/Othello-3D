using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UniRx;
using Unity.VisualScripting;
using UnityEngine;

namespace GameSystem
{
     public class BoardCell : MonoBehaviour
     {
         [SerializeField] private GameObject _discPrefab;
         [SerializeField] private float _discYOffset = .5f;
         [SerializeField] private Light _light;
         private Board _board;
         private GameObject _disc;

         public int X { get; private set; }

         public int Y { get; private set; }
         // Start is called before the first frame update
         void Awake()
         {
             FindSelfPosition();
             _board = FindObjectOfType<Board>();
             _board.CellAsObservable(X, Y)
                 .Subscribe(v =>
                 {
                     var oldStatus = v.oldValue;
                     var newStatus = v.newValue;
                     if (oldStatus == CellStatus.Empty && newStatus == CellStatus.Black)
                     {
                         PlaceBlack();
                     }
                     else if (oldStatus == CellStatus.Empty && newStatus == CellStatus.White)
                     {
                         PlaceWhite();
                     }
                     else if (oldStatus == CellStatus.Black && newStatus == CellStatus.White)
                     {
                         FlipDiscToWhite();
                     } 
                     else if (oldStatus == CellStatus.White && newStatus == CellStatus.Black)
                     {
                         FlipDiscToBlack();
                     }
                 });
         }

         // めんどくさいので計算でx, y座標を出します
         void FindSelfPosition()
         {
             var position = transform.localPosition;
             X = (int)(position.x + 3.5);
             Y = (int)(position.z + 3.5);
             Debug.Log($"x = {X}, y = {Y}");
         }

         public void TurnOnHighlight()
         {
             _light.enabled = true;
         }

         public void TurnOffHighlight()
         {
             _light.enabled = false;
         }

         void FlipDiscToWhite()
         {
             _disc.GetComponent<Animator>().SetTrigger("flipToWhite");
         }
        void FlipDiscToBlack()
         {
             _disc.GetComponent<Animator>().SetTrigger("flipToBlack");
         }

         void PlaceWhite()
         {
             _disc = Instantiate(_discPrefab, transform.position + Vector3.up * _discYOffset, Quaternion.identity, transform);
             _disc.GetComponent<Animator>().SetTrigger("putWhite");
         }

         void PlaceBlack()
         {
             _disc = Instantiate(_discPrefab, transform.position + Vector3.up * _discYOffset, Quaternion.identity, transform);
             _disc.GetComponent<Animator>().SetTrigger("putBlack");
         }
     
         // Update is called once per frame
         void Update()
         {
             
         }
     }   
}

