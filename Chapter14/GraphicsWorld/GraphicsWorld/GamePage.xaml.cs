﻿using System;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GraphicsWorld
{
    public partial class GamePage : PhoneApplicationPage
    {
        ContentManager contentManager;
        GameTimer timer;
        SpriteBatch spriteBatch;
        GamePlayComponent gamePlay = new GamePlayComponent();
        IGamePlayInput input;
        int frameCountAfterMotionStopped;

        UIElementRenderer scoreboardRenderer;
        Vector2 scoreboardPosition = new Vector2(12, 12);

        public GamePage()
        {
            InitializeComponent();

            // Get the content manager from the application
            contentManager = (Application.Current as App).Content;

            // Create a timer for this page
            timer = new GameTimer();
            timer.UpdateInterval = TimeSpan.FromTicks(333333);
            timer.Update += OnUpdate;
            timer.Draw += OnDraw;

            scoreboardRenderer = new UIElementRenderer(scoreboard, 456, 60);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Set the sharing mode of the graphics device to turn on XNA rendering
            SharedGraphicsDeviceManager.Current.GraphicsDevice.SetSharingMode(true);

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(SharedGraphicsDeviceManager.Current.GraphicsDevice);

            // TODO: use this.content to load your game content here
            input = new DemoInput();
            gamePlay.Initialize(contentManager, input);

            // Start the timer
            timer.Start();

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            // Stop the timer
            timer.Stop();

            // Set the sharing mode of the graphics device to turn off XNA rendering
            SharedGraphicsDeviceManager.Current.GraphicsDevice.SetSharingMode(false);

            base.OnNavigatedFrom(e);
        }

        /// <summary>
        /// Allows the page to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        private void OnUpdate(object sender, GameTimerEventArgs e)
        {
            // TODO: Add your update logic here
            input.Update();
            gamePlay.Update();
            score.Text = gamePlay.Score.ToString();

            if (!input.MoveForward && !input.MoveBackward
                && !input.TurnLeft && !input.TurnRight)
            {
                if (frameCountAfterMotionStopped > 2)
                {
                    GameTimer.SuppressFrame();
                }
            }
            else
            {
                frameCountAfterMotionStopped = 0;
            }

            if (!gamePlay.IsPlaying && NavigationService.CanGoBack)
                NavigationService.GoBack();
        }

        /// <summary>
        /// Allows the page to draw itself.
        /// </summary>
        private void OnDraw(object sender, GameTimerEventArgs e)
        {
            scoreboardRenderer.Render();

            var device = SharedGraphicsDeviceManager.Current.GraphicsDevice;
            device.Clear(Color.CornflowerBlue);
            device.DepthStencilState = DepthStencilState.Default;
            device.RasterizerState = RasterizerState.CullCounterClockwise;
            gamePlay.Draw();
            
            spriteBatch.Begin();
            spriteBatch.Draw(scoreboardRenderer.Texture, scoreboardPosition, Color.White);
            spriteBatch.End();
        }

    }
}
