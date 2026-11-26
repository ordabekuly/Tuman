using UnityEngine;
using UnityEngine.EventSystems;
namespace TumanGame
{
    public sealed class HoldFire:MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IPointerExitHandler
    {
        public PlayerShooting shooting;bool held;
        public void OnPointerDown(PointerEventData e){held=true;shooting.Fire();}
        public void OnPointerUp(PointerEventData e){held=false;}
        public void OnPointerExit(PointerEventData e){held=false;}
        void Update(){if(held)shooting.Fire();}
        void OnDisable(){held=false;}
    }
}
