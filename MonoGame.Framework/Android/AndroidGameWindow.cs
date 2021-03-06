#region License
/*
Microsoft Public License (Ms-PL)
XnaTouch - Copyright © 2009 The XnaTouch Team

All rights reserved.

This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
accept the license, do not use the software.

1. Definitions
The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the software.
A "contributor" is any person that distributes its contribution under this license.
"Licensed patents" are a contributor's patent claims that read directly on its contribution.

2. Grant of Rights
(A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
(B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

3. Conditions and Limitations
(A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
(B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
your patent license from such contributor to the software ends automatically.
(C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
notices that are present in the software.
(D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
code form, you may only do so under a license that complies with this license.
(E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
purpose and non-infringement.
*/
#endregion License

#region Using Statements
using System;
using System.Drawing;
using System.Collections.Generic;
using System.ComponentModel;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Util;
using Android.Views;
using Microsoft.Xna.Framework.Graphics;
using OpenTK.Platform.Android;

using OpenTK;
using OpenTK.Platform;
using OpenTK.Graphics;
//using OpenTK.Graphics.ES11;
//using OpenTK.Graphics.ES20;

using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
#endregion Using Statements

namespace Microsoft.Xna.Framework
{
    public class AndroidGameWindow : AndroidGameView, Android.Views.View.IOnTouchListener
    {
		private Rectangle clientBounds;
		internal Game game;
		private GameTime _updateGameTime;
        private GameTime _drawGameTime;
        private DateTime _lastUpdate;
		private DateTime _now;
        private DisplayOrientation _currentOrientation;
		private GestureDetector gesture = null;

        public AndroidGameWindow(Context context) :base(context)
        {          
            Initialize();							
        }
		
						
        private void Initialize()
        {
            
			this.Closed +=	new EventHandler<EventArgs>(GameWindow_Closed);            
			clientBounds = new Rectangle(0, 0, Context.Resources.DisplayMetrics.WidthPixels, Context.Resources.DisplayMetrics.HeightPixels);

            // Initialize GameTime
            _updateGameTime = new GameTime();
            _drawGameTime = new GameTime();

            // Initialize _lastUpdate
            _lastUpdate = DateTime.Now;
					
			gesture = new GestureDetector(new GestureListener((AndroidGameActivity)this.Context));
			
            this.RequestFocus();
            this.FocusableInTouchMode = true;

            this.SetOnTouchListener(this);
        }
		
		void GameWindow_Closed(object sender,EventArgs e)
        {        
			try
			{
        		game.Exit();
			}
			catch(NullReferenceException)
			{
				// just in case the game is null
			}
		}

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            Keyboard.KeyDown(keyCode);
            // we need to handle the Back key here because it doesnt work any other way
            if (keyCode == Keycode.Back) game.Exit();
            return true;
        }

        public override bool OnKeyUp(Keycode keyCode, KeyEvent e)
        {
            Keyboard.KeyUp(keyCode);
            return true;
        }

        ~AndroidGameWindow()
		{
			//
		}
		
		protected override void CreateFrameBuffer()
		{	    
			try
            {
                GLContextVersion = GLContextVersion.Gles2_0;
                base.CreateFrameBuffer();
		    } 
			catch (Exception) 
			{
		        //device doesn't support OpenGLES 2.0; retry with 1.1:
                GLContextVersion = GLContextVersion.Gles1_1;
				base.CreateFrameBuffer();
		    }
            GraphicsDevice.OpenGLESVersion = GLContextVersion;
		}
	

        #region AndroidGameView Methods

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            
            if (GraphicsContext == null || GraphicsContext.IsDisposed)
                return;

            //Should not happen at all..
            if (!GraphicsContext.IsCurrent)
                MakeCurrent();

            if (game != null) {
                _drawGameTime.Update(_now - _lastUpdate);
                _lastUpdate = _now;
                game.DoDraw(_drawGameTime);
            }
            try
            {
                SwapBuffers();
            }
            catch(Exception ex)
            {
                Android.Util.Log.Error("Error in swap buffers", ex.ToString());
            }
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
		{			
			base.OnUpdateFrame(e);
			
			if (game != null )
			{
                //ObserveDeviceRotation();

				_now = DateTime.Now;
				_updateGameTime.Update(_now - _lastUpdate);
            	game.DoUpdate(_updateGameTime);
			}
		}
		
		#endregion
		
		

        internal void SetOrientation(DisplayOrientation currentorientation)
        {
            if (game.graphicsDeviceManager == null)
                return;

            // Calculate supported orientations if it has been left as "default" and only default
            DisplayOrientation supportedOrientations = (game.graphicsDeviceManager as GraphicsDeviceManager).SupportedOrientations;			
			var allowedOrientation = DisplayOrientation.LandscapeLeft; 				
			if ((supportedOrientations == DisplayOrientation.Default))
			{
			  // if we have default only we only allow Landscape
			  allowedOrientation = allowedOrientation | DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight; 				
			}
			if ((supportedOrientations == DisplayOrientation.LandscapeLeft))
			{
			  // if we have default only we only allow Landscape
			  allowedOrientation = DisplayOrientation.LandscapeLeft; 				
			}
			if ((supportedOrientations & DisplayOrientation.LandscapeLeft) != 0)
			{
			  // if we have default only we only allow Landscape
			  allowedOrientation = allowedOrientation | DisplayOrientation.LandscapeLeft; 				
			}
			if ((supportedOrientations == DisplayOrientation.LandscapeRight))
			{
			  // if we have default only we only allow Landscape
			  allowedOrientation = DisplayOrientation.LandscapeRight; 				
			}
			if ((supportedOrientations & DisplayOrientation.LandscapeRight) != 0)
			{
			  // if we have default only we only allow Landscape
			  allowedOrientation = allowedOrientation | DisplayOrientation.LandscapeRight; 				
			}
			if ((supportedOrientations == DisplayOrientation.Portrait))
			{
			  // if we have Portrait only we only allow Landscape
			  allowedOrientation = DisplayOrientation.Portrait; 				
			}
			if ((supportedOrientations & DisplayOrientation.Portrait) != 0)
			{
			  // if we have default only we only allow Landscape
			  allowedOrientation = allowedOrientation | DisplayOrientation.Portrait; 				
			}
			
			// ok we default to landscape left
			var actualOrientation = DisplayOrientation.LandscapeLeft;
			// now based on the  orientation of the device we 
			// decide of we honour the device orientation or force our own
			
			// so if we are in Portrait but we allow only LandScape we stay in landscape
			if (allowedOrientation == DisplayOrientation.Portrait)
			{
				actualOrientation = DisplayOrientation.Portrait;
			}
			else
			if (allowedOrientation == DisplayOrientation.LandscapeLeft)
			{
				actualOrientation = DisplayOrientation.LandscapeLeft;
			}
			else
			if (allowedOrientation == DisplayOrientation.LandscapeRight)
			{
				actualOrientation = DisplayOrientation.LandscapeRight;
			}	
			
            switch (currentorientation) {

			case DisplayOrientation.Portrait:
                    if ((allowedOrientation & DisplayOrientation.Portrait) != 0) {
                        actualOrientation = DisplayOrientation.Portrait;
                    }
                    break;
				case DisplayOrientation.LandscapeRight:	
				    if ((allowedOrientation & DisplayOrientation.LandscapeRight) != 0) {
                        actualOrientation = DisplayOrientation.LandscapeRight;
                    }				    
				    break;
                case DisplayOrientation.LandscapeLeft:				     
                default:
					if ((allowedOrientation & DisplayOrientation.LandscapeLeft) != 0) {
				    	actualOrientation = DisplayOrientation.LandscapeLeft;
					}
                    break;
            }
			
			
			CurrentOrientation = actualOrientation;
            game.GraphicsDevice.PresentationParameters.DisplayOrientation = actualOrientation;
            TouchPanel.DisplayOrientation = actualOrientation;
        }

        private Dictionary<IntPtr, TouchLocation> _previousTouches = new Dictionary<IntPtr, TouchLocation>();

        public override bool OnTouchEvent(MotionEvent e)
        {
            return false;
        }

        public bool OnTouch(View v, MotionEvent e)
        {
            TouchLocation tlocation;            
            TouchCollection collection = TouchPanel.Collection;            
            Vector2 position = Vector2.Zero;            
            position.X = e.GetX(e.ActionIndex);            
            position.Y = e.GetY(e.ActionIndex);            
            int id = e.GetPointerId(e.ActionIndex);            
            int index;            
            switch (e.ActionMasked)            
            {                
                // DOWN                
                case 0:                
                case 5:                    
                    tlocation = new TouchLocation(id, TouchLocationState.Pressed, position);                    
                    collection.Add(tlocation);                    
                    break;                
                // UP                
                case 1:                
                case 6:                    
                    index = collection.FindById(e.GetPointerId(e.ActionIndex), out tlocation);                    
                    if (index >= 0)                    
                    {                        
                        tlocation.State = TouchLocationState.Released;                        
                        collection[index] = tlocation;                    
                    }                    
                    break;                
                // MOVE                
                case 2:                    
                    for (int i = 0; i < e.PointerCount; i++)                    
                    {                        
                        id = e.GetPointerId(i);                        
                        position.X = e.GetX(i);                        
                        position.Y = e.GetY(i);                        
                        index = collection.FindById(id, out tlocation);                        
                        if (index >= 0)                        
                        {                            
                            tlocation.State = TouchLocationState.Moved;                            
                            tlocation.Position = position;                            
                            collection[index] = tlocation;                        
                        }                    
                    }                    
                    break;                
                // CANCEL, OUTSIDE                
                case 3:                
                case 4:                    
                    index = collection.FindById(id, out tlocation);                    
                    if (index >= 0)                    
                    {                        
                        tlocation.State = TouchLocationState.Invalid;                        
                        collection[index] = tlocation;                    
                    }                    
                    break;            
            }            
            return true;
        }
        
        public string ScreenDeviceName 
		{
			get 
			{
				throw new System.NotImplementedException ();
			}
		}
   

        public Rectangle ClientBounds 
		{
			get 
			{
				return clientBounds;
			}
		}
		
		public bool AllowUserResizing 
		{
			get 
			{
				return false;
			}
			set 
			{
				// Do nothing; Ignore rather than raising and exception
			}
		}
        
		public DisplayOrientation CurrentOrientation 
		{
            get
            {
                return _currentOrientation;
            }
            private set
            {
                if (value != _currentOrientation)
                {
                    _currentOrientation = value;

                    if (_currentOrientation == DisplayOrientation.Portrait || _currentOrientation == DisplayOrientation.PortraitUpsideDown)
                        Game.contextInstance.SetRequestedOrientation(ScreenOrientation.Portrait);
                    else if (_currentOrientation == DisplayOrientation.LandscapeLeft || _currentOrientation == DisplayOrientation.LandscapeRight)
                        Game.contextInstance.SetRequestedOrientation(ScreenOrientation.Landscape);

                    if (OrientationChanged != null)
                    {
                        OrientationChanged(this, EventArgs.Empty);
                    }
                }
            }
		}

        public event EventHandler<EventArgs> OrientationChanged;
		public event EventHandler ClientSizeChanged;
		public event EventHandler ScreenDeviceNameChanged;

    }
}

