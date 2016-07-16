/***********************************************
				EasyTouch V
	Copyright © 2014-2015 The Hedgehog Team
    http://www.thehedgehogteam.com/Forum/
		
	  The.Hedgehog.Team@gmail.com
		
**********************************************/
using UnityEngine;
using System.Collections;

namespace HedgehogTeam.EasyTouch
{
    [System.Serializable]
    public class ECamera {

	    public Camera camera;
	    public bool guiCamera;
        public LayerMask pickableLayers3D;
        public LayerMask pickableLayers2D;
        public bool enable3D;
        public bool enable2D;
        public bool useGlobalLayers;

        //--------------------
        // Editor
        //--------------------
        public bool showLayerOptions = true;

        public ECamera( Camera cam, bool gui){
		    camera = cam;
		    guiCamera = gui;

            pickableLayers3D = 1 << 0;
            pickableLayers2D = 1 << 0;
            enable3D = true;
            enable2D = false;
            useGlobalLayers = false;
        }
	
    }
}
