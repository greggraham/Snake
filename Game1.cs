using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;

namespace Snake
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        enum GameStates
        {
            StartScreen,
            Play,
            EndScreen
        }

        enum Dir
        {
            Up, Down, Left, Right
        }

        struct GridLoc
        {
            public int row, col;
            public GridLoc(int r, int c) { row = r; col = c; }
            public GridLoc(GridLoc oldPos, Dir d)
            {
                row = oldPos.row;
                col = oldPos.col;
                switch (d)
                {
                    case Dir.Up:
                        row--;
                        break;
                    case Dir.Down:
                        row++;
                        break;
                    case Dir.Left:
                        col--;
                        break;
                    case Dir.Right:
                        col++;
                        break;
                }
            }
        }

        private const int grideSize = 16;
        private const int gridWidth = 64;
        private const int gridHeight = 50;
        private const int width = grideSize * gridWidth;
        private const int height = grideSize * gridHeight;

        private TimeSpan moveDelay = TimeSpan.FromSeconds(0.15);
        private TimeSpan moveTimer = TimeSpan.FromSeconds(0);

        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private Texture2D p1Texture;
        private Texture2D p2Texture;
        private SpriteFont font;

        private List<GridLoc> p1Snake;
        private List<GridLoc> p2Snake;
        private Dir p1Dir;
        private Dir p2Dir;
        private bool p1Crashed;
        private bool p2Crashed;

        private GameStates state = GameStates.StartScreen;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferHeight = height,
                PreferredBackBufferWidth = width
            };
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            InitializePlayers();

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            p1Texture = Content.Load<Texture2D>("snake1");
            p2Texture = Content.Load<Texture2D>("snake2");
            font = Content.Load<SpriteFont>("Message");
            Song song = Content.Load<Song>("Bonkers-for-Arcades");
            MediaPlayer.Play(song);
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = 0.5f;
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            var kstate = Keyboard.GetState();

            switch (state)
            {
                case GameStates.StartScreen:
                    if (kstate.IsKeyDown(Keys.Space))
                    {
                        state = GameStates.Play;
                    }
                    break;

                case GameStates.Play:
                    CheckDirKeys(kstate);
                    if (gameTime.TotalGameTime > moveTimer)
                    {
                        var new1 = new GridLoc(p1Snake[p1Snake.Count - 1], p1Dir);
                        var new2 = new GridLoc(p2Snake[p2Snake.Count - 1], p2Dir);
                        p1Crashed = CheckCrash(new1);
                        p2Crashed = CheckCrash(new2);
                        if (p1Crashed || p2Crashed)
                        {
                            state = GameStates.EndScreen;
                        }
                        else
                        {
                            p1Snake.Add(new1);
                            p2Snake.Add(new2);
                            moveTimer = gameTime.TotalGameTime + moveDelay;
                        }
                    }
                    break;

                case GameStates.EndScreen:
                    if (kstate.IsKeyDown(Keys.Y))
                    {
                        InitializePlayers();
                        state = GameStates.StartScreen;
                    }
                    else if (kstate.IsKeyDown(Keys.N))
                    {
                        Exit();
                    }
                    break;
            }
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkSlateBlue);

            // TODO: Add your drawing code here
            spriteBatch.Begin();
            switch (state)
            {
                case GameStates.StartScreen:
                    spriteBatch.DrawString(font, "Snake", new Vector2(100, 100), Color.White);
                    spriteBatch.DrawString(font, "Press Space to start.", new Vector2(100, 150), Color.White);
                    break;

                case GameStates.Play:
                    DrawSnake(p1Snake, p1Texture);
                    DrawSnake(p2Snake, p2Texture);
                    break;

                case GameStates.EndScreen:
                    spriteBatch.DrawString(font, "Game Over.", new Vector2(100, 100), Color.White);
                    string msg = (p1Crashed ? "Player 1 crashed. " : "") + (p2Crashed ? "Player 2 crashed." : "");
                    spriteBatch.DrawString(font, msg, new Vector2(100, 150), Color.White);
                    spriteBatch.DrawString(font, "Do you want to play again (Y/N)?", new Vector2(100, 200), Color.White);
                    break;
            }
            spriteBatch.End();
            base.Draw(gameTime);
        }


        private void InitializePlayers()
        {
            p1Snake = new List<GridLoc>(1000)
            {
                new GridLoc(gridHeight / 2, 0)
            };
            p1Dir = Dir.Right;
            p1Crashed = false;
            p2Snake = new List<GridLoc>(1000)
            {
                new GridLoc(gridHeight / 2, gridWidth - 1)
            };
            p2Dir = Dir.Left;
            p2Crashed = false;
        }

        private void CheckDirKeys(KeyboardState kstate)
        {
            if (kstate.IsKeyDown(Keys.A))
            {
                p1Dir = Dir.Left;
            }
            else if (kstate.IsKeyDown(Keys.S))
            {
                p1Dir = Dir.Down;
            }
            else if (kstate.IsKeyDown(Keys.D))
            {
                p1Dir = Dir.Right;
            }
            else if (kstate.IsKeyDown(Keys.W))
            {
                p1Dir = Dir.Up;
            }

            if (kstate.IsKeyDown(Keys.Left))
            {
                p2Dir = Dir.Left;
            }
            else if (kstate.IsKeyDown(Keys.Down))
            {
                p2Dir = Dir.Down;
            }
            else if (kstate.IsKeyDown(Keys.Right))
            {
                p2Dir = Dir.Right;
            }
            else if (kstate.IsKeyDown(Keys.Up))
            {
                p2Dir = Dir.Up;
            }

        }


        private bool CheckBorderCrash(GridLoc pos)
        {
            return pos.row <= 0 || pos.row >= height || pos.col <= 0 || pos.col >= width;
        }


        private bool CheckSnakeCrash(GridLoc pos, List<GridLoc> snake)
        {
            foreach (var snakePos in snake)
            {
                if (pos.row == snakePos.row && pos.col == snakePos.col)
                {
                    return true;
                }
            }
            return false;
        }


        private bool CheckCrash(GridLoc pos)
        {
            return CheckBorderCrash(pos) || CheckSnakeCrash(pos, p1Snake) || CheckSnakeCrash(pos, p2Snake);
        }


        private void DrawSnake(List<GridLoc> snake, Texture2D cell)
        {
            foreach (var pos in snake)
            {
                var loc = new Vector2(pos.col * grideSize, pos.row * grideSize);
                spriteBatch.Draw(cell, loc, Color.White);
            }
        }
    }
}
