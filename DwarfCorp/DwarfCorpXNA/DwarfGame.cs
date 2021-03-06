// DwarfGame.cs
// 
//  Modified MIT License (MIT)
//  
//  Copyright (c) 2015 Completely Fair Games Ltd.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// The following content pieces are considered PROPRIETARY and may not be used
// in any derivative works, commercial or non commercial, without explicit 
// written permission from Completely Fair Games:
// 
// * Images (sprites, textures, etc.)
// * 3D Models
// * Sound Effects
// * Music
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Threading;
using DwarfCorp.GameStates;
using Gum;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;


namespace DwarfCorp
{

    public class DwarfGame : Game
    {
        public static bool COMPRESSED_BINARY_SAVES = false;

        public GameStateManager StateManager { get; set; }
        public GraphicsDeviceManager Graphics;
        public TextureManager TextureManager { get; set; }
        public static SpriteBatch SpriteBatch { get; set; }

        public static Gum.Input.GumInputMapper GumInputMapper;
        public static Gum.Input.Input GumInput;
        public static Gum.RenderData GumSkin;
        public DwarfGame()
        {
            GameState.Game = this;
            Content.RootDirectory = "Content";
            StateManager = new GameStateManager(this);
            Graphics = new GraphicsDeviceManager(this);
            Window.Title = "DwarfCorp";
            Window.AllowUserResizing = false;
            TextureManager = new TextureManager(Content, GraphicsDevice);
            GameSettings.Load();
            Graphics.IsFullScreen = GameSettings.Default.Fullscreen;
            Graphics.PreferredBackBufferWidth = GameSettings.Default.ResolutionX;
            Graphics.PreferredBackBufferHeight = GameSettings.Default.ResolutionY;

            try
            {
                Graphics.ApplyChanges();
            }
            catch(NoSuitableGraphicsDeviceException exception)
            {
                Console.Error.WriteLine(exception.Message);
            }
        }

        public static string GetGameDirectory()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + ProgramData.DirChar + "DwarfCorp";
        }

        protected override void Initialize()
        {
            Thread.CurrentThread.Name = "Main";
            // Goes before anything else so we can track from the very start.
            GamePerformance.Initialize(this);
            // TODO: Find a more appropriate spot for this.
            GameObjectCaching.Initialize();

            SpriteBatch = new SpriteBatch(GraphicsDevice);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Prepare GemGui
            GumInputMapper = new Gum.Input.GumInputMapper(Window.Handle);
            GumInput = new Gum.Input.Input(GumInputMapper);

            // Register all bindable actions with the input system.
            GumInput.AddAction("TEST", Gum.Input.KeyBindingType.Pressed);

            GumSkin = new RenderData(GraphicsDevice,  Content,
                    "newgui/xna_draw", "Content/newgui/sheets.txt");

            if (SoundManager.Content == null)
            {
                SoundManager.Content = Content;
                SoundManager.LoadDefaultSounds();
#if XNA_BUILD
                SoundManager.SetActiveSongs(ContentPaths.Music.dwarfcorp, ContentPaths.Music.dwarfcorp_2,
                    ContentPaths.Music.dwarfcorp_3, ContentPaths.Music.dwarfcorp_4, ContentPaths.Music.dwarfcorp_5);
#endif
            }

            // The thing keeping this from working is that some states are tied tightly to the play state.
            // Ideally the solution is to stop caching these at all, so there's no point in trying to make
            // an implementation work just to throw it out.
            /*
            foreach (var type in System.Reflection.Assembly.GetExecutingAssembly().GetTypes())
            {
                if (type.IsSubclassOf(typeof(GameState)))
                {
                    var instance = Activator.CreateInstance(type, this, StateManager);
                    StateManager.States.Add(type.Name, instance as GameState);
                }
            }
            */

            /*
            PlayState playState = new PlayState(this, StateManager);
            StateManager.States["IntroState"] = new IntroState(this, StateManager);
            StateManager.States["PlayState"] = playState;
            StateManager.States["MainMenuState"] = new MainMenuState(this, StateManager);
            StateManager.States["NewGameChooseWorldState"] = new NewGameChooseWorldState(this, StateManager);
            StateManager.States["NewGameCreateDebugWorldState"] = new NewGameCreateDebugWorldState(this, StateManager);
            StateManager.States["WorldSetupState"] = new WorldSetupState(this, StateManager);
            StateManager.States["WorldGeneratorState"] = new WorldGeneratorState(this, StateManager);
            StateManager.States["OptionsState"] = new OptionsState(this, StateManager);
            StateManager.States["NewOptionsState"] = new NewOptionsState(this, StateManager);
            StateManager.States["EconomyState"] = new EconomyState(this, StateManager);
            StateManager.States["CompanyMakerState"] = new CompanyMakerState(this, StateManager);
            StateManager.States["WorldLoaderState"] = new WorldLoaderState(this, StateManager);
            StateManager.States["GameLoaderState"] = new GameLoaderState(this, StateManager);
            StateManager.States["LoseState"] = new LoseState(this, StateManager, playState);
            StateManager.States["LoadState"] = new LoadState(this, StateManager);
             */

            if(GameSettings.Default.DisplayIntro)
            {
                StateManager.PushState(new IntroState(this, StateManager));
            }
            else
            {
                StateManager.PushState(new MainMenuState(this, StateManager));
            }

            BiomeLibrary.InitializeStatics();
            Embarkment.Initialize();
            VoxelChunk.InitializeStatics();
            ControlSettings.Load();
            Drawer2D.Initialize(Content, GraphicsDevice);
            ResourceLibrary.Initialize();

            base.LoadContent();
        }

        protected override void Update(GameTime time)
        {
            GamePerformance.Instance.PreUpdate();
            DwarfTime.LastTime.Update(time);
            StateManager.Update(DwarfTime.LastTime);
            base.Update(time);
            GamePerformance.Instance.PostUpdate();
        }

        protected override void Draw(GameTime time)
        {
            GamePerformance.Instance.PreRender();
            StateManager.Render(DwarfTime.LastTime);
            GraphicsDevice.SetRenderTarget(null);
            base.Draw(time);
            GamePerformance.Instance.PostRender();
            GamePerformance.Instance.Render(SpriteBatch);
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            ExitGame = true;
            Program.SignalShutdown();
            base.OnExiting(sender, args);
        }

        public static bool ExitGame = false;
    }

}
