using UnityEngine;

namespace UnitySampleAssets.CrossPlatformInput
{
    public class ButtonHandler : MonoBehaviour
    {
		public float origW = 546;
		public float origH = 307;
		private Vector3 startPos;
		
    	void Start()
    	{
				
			/*float scaleX = (float)(Screen.width) / origW;
			float scaleY = (float)(Screen.height) / origH;
			float startPosX = (float)Screen.width - (float)Screen.width/5f;
			float startPosY = (float)Screen.height/8f;
			transform.localScale = new Vector3(scaleX*transform.localScale.x, scaleY*transform.localScale.y, 1);
			startPos = new Vector3(startPosX,startPosY,transform.position.z); //transform.position + new Vector3(scaleX, scaleY, 0);
			transform.position = startPos;*/
    	}
    	
        public void SetDownState(string name)
        {
            CrossPlatformInputManager.SetButtonDown(name);
        }


        public void SetUpState(string name)
        {
            CrossPlatformInputManager.SetButtonUp(name);
        }


        public void SetAxisPositiveState(string name)
        {
            CrossPlatformInputManager.SetAxisPositive(name);
        }


        public void SetAxisNeutralState(string name)
        {
            CrossPlatformInputManager.SetAxisZero(name);
        }


        public void SetAxisNegativeState(string name)
        {
            CrossPlatformInputManager.SetAxisNegative(name);
        }
    }
}