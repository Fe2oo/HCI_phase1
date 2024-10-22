/*
	TUIO C# Demo - part of the reacTIVision project
	Copyright (c) 2005-2016 Martin Kaltenbrunner <martin@tuio.org>

	This program is free software; you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation; either version 2 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with this program; if not, write to the Free Software
	Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*/

using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections;
using System.Threading;
using TUIO;
using System.IO;

public class TuioDemo : Form, TuioListener
{
	private TuioClient client;
	private Dictionary<long, TuioObject> objectList;
	private Dictionary<long, TuioCursor> cursorList;
	private Dictionary<long, TuioBlob> blobList;

	public static int width, height;
	private int window_width = 700;
	private int window_height = 300;
	private int window_left = 0;
	private int window_top = 0;
	private int screen_width = Screen.PrimaryScreen.Bounds.Width;
	private int screen_height = Screen.PrimaryScreen.Bounds.Height;

	private bool fullscreen;
	private bool verbose;

	Font font = new Font("Arial", 10.0f);
	SolidBrush fntBrush = new SolidBrush(Color.White);
	SolidBrush bgrBrush = new SolidBrush(Color.FromArgb(0, 0, 64));
	SolidBrush curBrush = new SolidBrush(Color.FromArgb(192, 0, 192));
	SolidBrush objBrush = new SolidBrush(Color.FromArgb(64, 0, 0));
	SolidBrush blbBrush = new SolidBrush(Color.FromArgb(64, 64, 64));
	Pen curPen = new Pen(new SolidBrush(Color.Blue), 1);

	public TuioDemo(int port)
	{

		verbose = false;
		fullscreen = false;
		width = window_width;
		height = window_height;

		this.ClientSize = new System.Drawing.Size(width, height);
		this.Name = "TuioDemo";
		this.Text = "TuioDemo";

		this.Closing += new CancelEventHandler(Form_Closing);
		this.KeyDown += new KeyEventHandler(Form_KeyDown);

		this.SetStyle(ControlStyles.AllPaintingInWmPaint |
						ControlStyles.UserPaint |
						ControlStyles.DoubleBuffer, true);

		objectList = new Dictionary<long, TuioObject>(128);
		cursorList = new Dictionary<long, TuioCursor>(128);
		blobList = new Dictionary<long, TuioBlob>(128);

		client = new TuioClient(port);
		client.addTuioListener(this);

		client.connect();
	}

	private void Form_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
	{

		if (e.KeyData == Keys.F1)
		{
			if (fullscreen == false)
			{

				width = screen_width;
				height = screen_height;

				window_left = this.Left;
				window_top = this.Top;

				this.FormBorderStyle = FormBorderStyle.None;
				this.Left = 0;
				this.Top = 0;
				this.Width = screen_width;
				this.Height = screen_height;

				fullscreen = true;
			}
			else
			{

				width = window_width;
				height = window_height;

				this.FormBorderStyle = FormBorderStyle.Sizable;
				this.Left = window_left;
				this.Top = window_top;
				this.Width = window_width;
				this.Height = window_height;

				fullscreen = false;
			}
		}
		else if (e.KeyData == Keys.Escape)
		{
			this.Close();

		}
		else if (e.KeyData == Keys.V)
		{
			verbose = !verbose;
		}

	}

	private void Form_Closing(object sender, System.ComponentModel.CancelEventArgs e)
	{
		client.removeTuioListener(this);

		client.disconnect();
		System.Environment.Exit(0);
	}

	public void addTuioObject(TuioObject o)
	{
		lock (objectList)
		{
			objectList.Add(o.SessionID, o);
		}
		if (verbose) Console.WriteLine("add obj " + o.SymbolID + " (" + o.SessionID + ") " + o.X + " " + o.Y + " " + o.Angle);
	}

	public void updateTuioObject(TuioObject o)
	{

		if (verbose) Console.WriteLine("set obj " + o.SymbolID + " " + o.SessionID + " " + o.X + " " + o.Y + " " + o.Angle + " " + o.MotionSpeed + " " + o.RotationSpeed + " " + o.MotionAccel + " " + o.RotationAccel);
	}

	public void removeTuioObject(TuioObject o)
	{
		lock (objectList)
		{
			objectList.Remove(o.SessionID);
		}
		if (verbose) Console.WriteLine("del obj " + o.SymbolID + " (" + o.SessionID + ")");
	}

	public void addTuioCursor(TuioCursor c)
	{
		lock (cursorList)
		{
			cursorList.Add(c.SessionID, c);
		}
		if (verbose) Console.WriteLine("add cur " + c.CursorID + " (" + c.SessionID + ") " + c.X + " " + c.Y);
	}

	public void updateTuioCursor(TuioCursor c)
	{
		if (verbose) Console.WriteLine("set cur " + c.CursorID + " (" + c.SessionID + ") " + c.X + " " + c.Y + " " + c.MotionSpeed + " " + c.MotionAccel);
	}

	public void removeTuioCursor(TuioCursor c)
	{
		lock (cursorList)
		{
			cursorList.Remove(c.SessionID);
		}
		if (verbose) Console.WriteLine("del cur " + c.CursorID + " (" + c.SessionID + ")");
	}

	public void addTuioBlob(TuioBlob b)
	{
		lock (blobList)
		{
			blobList.Add(b.SessionID, b);
		}
		if (verbose) Console.WriteLine("add blb " + b.BlobID + " (" + b.SessionID + ") " + b.X + " " + b.Y + " " + b.Angle + " " + b.Width + " " + b.Height + " " + b.Area);
	}

	public void updateTuioBlob(TuioBlob b)
	{

		if (verbose) Console.WriteLine("set blb " + b.BlobID + " (" + b.SessionID + ") " + b.X + " " + b.Y + " " + b.Angle + " " + b.Width + " " + b.Height + " " + b.Area + " " + b.MotionSpeed + " " + b.RotationSpeed + " " + b.MotionAccel + " " + b.RotationAccel);
	}

	public void removeTuioBlob(TuioBlob b)
	{
		lock (blobList)
		{
			blobList.Remove(b.SessionID);
		}
		if (verbose) Console.WriteLine("del blb " + b.BlobID + " (" + b.SessionID + ")");
	}

	public void refresh(TuioTime frameTime)
	{
		Invalidate();
	}

	protected override void OnPaintBackground(PaintEventArgs pevent)
	{
		// Getting the graphics object
		Graphics g = pevent.Graphics;
		g.FillRectangle(bgrBrush, new Rectangle(0, 0, width, height));

		// Draw the cursor path as usual
		// (existing cursor drawing code...)

		// Draw the objects
		if (objectList.Count > 0)
		{
			lock (objectList)
			{
				foreach (TuioObject tobj in objectList.Values)
				{
					int ox = tobj.getScreenX(width);
					int oy = tobj.getScreenY(height);
					int size = height / 2;


					if (tobj.SymbolID == 0)
					{
						string tiger = Path.Combine(Environment.CurrentDirectory, "tiger.png");
						string forest = Path.Combine(Environment.CurrentDirectory, "forest.jpg");
						string error = Path.Combine(Environment.CurrentDirectory, "error.png");

						try
						{
							// Draw first image
							if (File.Exists(tiger))
							{
								using (Image img1 = Image.FromFile(tiger))
								{
									g.DrawImage(img1, new Rectangle(ox - size / 2, oy - size / 2, size, size));
								}
							}

							else
							{
								using (Image img3 = Image.FromFile(error))
								{
									g.DrawImage(img3, new Rectangle(ox - size / 2, oy - size / 2, size, size));
								}
							}

							// Draw second image (next to the first image)
							if (File.Exists(forest))
							{
								using (Image img2 = Image.FromFile(forest))
								{
									g.DrawImage(img2, new Rectangle(ox + size, oy - size / 2, size, size));
								}
							}
							else
							{
								using (Image img3 = Image.FromFile(error))
								{
									g.DrawImage(img3, new Rectangle(ox - size / 2, oy - size / 2, size, size));
								}
							}
						}
						catch (Exception ex)
						{
							Console.WriteLine($"Error loading images: {ex.Message}");
						}
					}
					else if (tobj.SymbolID == 1)
					{
						string lion = Path.Combine(Environment.CurrentDirectory, "lion.png");
						string forest = Path.Combine(Environment.CurrentDirectory, "forest.jpg");
						string error = Path.Combine(Environment.CurrentDirectory, "error.png");


						try
						{
							// Draw first image
							if (File.Exists(lion))
							{
								using (Image img1 = Image.FromFile(lion))
								{
									g.DrawImage(img1, new Rectangle(ox - size / 2, oy - size / 2, size, size));
								}
							}
							else
							{
								using (Image img3 = Image.FromFile(error))
								{
									g.DrawImage(img3, new Rectangle(ox - size / 2, oy - size / 2, size, size));
								}
							}

							// Draw second image (next to the first image)
							if (File.Exists(forest))
							{
								using (Image img2 = Image.FromFile(forest))
								{
									g.DrawImage(img2, new Rectangle(ox + size, oy - size / 2, size, size));
								}
							}
							else
							{
								using (Image img3 = Image.FromFile(error))
								{
									g.DrawImage(img3, new Rectangle(ox - size / 2, oy - size / 2, size, size));
								}
							}
						}
						catch (Exception ex)
						{
							Console.WriteLine($"Error loading images: {ex.Message}");
						}
					}
					else if (tobj.SymbolID == 2)
					{
						string lion = Path.Combine(Environment.CurrentDirectory, "fox.png");
						string forest = Path.Combine(Environment.CurrentDirectory, "forest.jpg");
						string error = Path.Combine(Environment.CurrentDirectory, "error.png");

						try
						{
							// Draw first image
							if (File.Exists(lion))
							{
								using (Image img1 = Image.FromFile(lion))
								{
									g.DrawImage(img1, new Rectangle(ox - size / 2, oy - size / 2, size, size));
								}
							}
							else
							{
								using (Image img3 = Image.FromFile(error))
								{
									g.DrawImage(img3, new Rectangle(ox - size / 2, oy - size / 2, size, size));
								}
							}

							// Draw second image (next to the first image)
							if (File.Exists(forest))
							{
								using (Image img2 = Image.FromFile(forest))
								{
									g.DrawImage(img2, new Rectangle(ox + size, oy - size / 2, size, size));
								}
							}
							else
							{
								using (Image img3 = Image.FromFile(error))
								{
									g.DrawImage(img3, new Rectangle(ox - size / 2, oy - size / 2, size, size));
								}
							}
						}
						catch (Exception ex)
						{
							Console.WriteLine($"Error loading images: {ex.Message}");
						}
					}
					else if (tobj.SymbolID == 3)
					{
						string lion = Path.Combine(Environment.CurrentDirectory, "fish.jpg");
						string forest = Path.Combine(Environment.CurrentDirectory, "sea.jpg");
						string error = Path.Combine(Environment.CurrentDirectory, "error.png");

						try
						{
							// Draw first image
							if (File.Exists(lion))
							{
								using (Image img1 = Image.FromFile(lion))
								{
									g.DrawImage(img1, new Rectangle(ox - size / 2, oy - size / 2, size, size));
								}
							}
							else
							{
								using (Image img3 = Image.FromFile(error))
								{
									g.DrawImage(img3, new Rectangle(ox - size / 2, oy - size / 2, size, size));
								}
							}

							// Draw second image (next to the first image)
							if (File.Exists(forest))
							{
								using (Image img2 = Image.FromFile(forest))
								{
									g.DrawImage(img2, new Rectangle(ox + size, oy - size / 2, size, size));
								}
							}
							else
							{
								using (Image img3 = Image.FromFile(error))
								{
									g.DrawImage(img3, new Rectangle(ox - size / 2, oy - size / 2, size, size));
								}
							}
						}
						catch (Exception ex)
						{
							Console.WriteLine($"Error loading images: {ex.Message}");
						}
					}
					else if (tobj.SymbolID == 4)
					{
						string lion = Path.Combine(Environment.CurrentDirectory, "whale.jpg");
						string forest = Path.Combine(Environment.CurrentDirectory, "sea.jpg");
						string error = Path.Combine(Environment.CurrentDirectory, "error.png");

						try
						{
							// Draw first image
							if (File.Exists(lion))
							{
								using (Image img1 = Image.FromFile(lion))
								{
									g.DrawImage(img1, new Rectangle(ox - size / 2, oy - size / 2, size, size));
								}
							}
							else
							{
								using (Image img3 = Image.FromFile(error))
								{
									g.DrawImage(img3, new Rectangle(ox - size / 2, oy - size / 2, size, size));
								}
							}

							// Draw second image (next to the first image)
							if (File.Exists(forest))
							{
								using (Image img2 = Image.FromFile(forest))
								{
									g.DrawImage(img2, new Rectangle(ox + size, oy - size / 2, size, size));
								}
							}
							else
							{
								using (Image img3 = Image.FromFile(error))
								{
									g.DrawImage(img3, new Rectangle(ox - size / 2, oy - size / 2, size, size));
								}
							}
						}
						catch (Exception ex)
						{
							Console.WriteLine($"Error loading images: {ex.Message}");
						}
					}
					else if (tobj.SymbolID == 5)
					{
						string lion = Path.Combine(Environment.CurrentDirectory, "shark.jpg");
						string forest = Path.Combine(Environment.CurrentDirectory, "sea.jpg");
						string error = Path.Combine(Environment.CurrentDirectory, "error.png");

						try
						{
							// Draw first image
							if (File.Exists(lion))
							{
								using (Image img1 = Image.FromFile(lion))
								{
									g.DrawImage(img1, new Rectangle(ox - size / 2, oy - size / 2, size, size));
								}
							}
							else
							{
								using (Image img3 = Image.FromFile(error))
								{
									g.DrawImage(img3, new Rectangle(ox - size / 2, oy - size / 2, size, size));
								}
							}

							// Draw second image (next to the first image)
							if (File.Exists(forest))
							{
								using (Image img2 = Image.FromFile(forest))
								{
									g.DrawImage(img2, new Rectangle(ox + size, oy - size / 2, size, size));
								}
							}
							else
							{
								using (Image img3 = Image.FromFile(error))
								{
									g.DrawImage(img3, new Rectangle(ox - size / 2, oy - size / 2, size, size));
								}
							}
						}
						catch (Exception ex)
						{
							Console.WriteLine($"Error loading images: {ex.Message}");
						}
					}
					else if (tobj.SymbolID == 6)
					{
						string lion = Path.Combine(Environment.CurrentDirectory, "dog.jpg");
						string forest = Path.Combine(Environment.CurrentDirectory, "house.jpg");
						string error = Path.Combine(Environment.CurrentDirectory, "error.png");

						try
						{
							// Draw first image
							if (File.Exists(lion))
							{
								using (Image img1 = Image.FromFile(lion))
								{
									g.DrawImage(img1, new Rectangle(ox - size / 2, oy - size / 2, size, size));
								}
							}
							else
							{
								using (Image img3 = Image.FromFile(error))
								{
									g.DrawImage(img3, new Rectangle(ox - size / 2, oy - size / 2, size, size));
								}
							}

							// Draw second image (next to the first image)
							if (File.Exists(forest))
							{
								using (Image img2 = Image.FromFile(forest))
								{
									g.DrawImage(img2, new Rectangle(ox + size, oy - size / 2, size, size));
								}
							}
							else
							{
								using (Image img3 = Image.FromFile(error))
								{
									g.DrawImage(img3, new Rectangle(ox - size / 2, oy - size / 2, size, size));
								}
							}
						}
						catch (Exception ex)
						{
							Console.WriteLine($"Error loading images: {ex.Message}");
						}
					}
					else if (tobj.SymbolID == 7)
					{
						string lion = Path.Combine(Environment.CurrentDirectory, "far.jpg");
						string forest = Path.Combine(Environment.CurrentDirectory, "house.jpg");
						string error = Path.Combine(Environment.CurrentDirectory, "error.png");

						try
						{
							// Draw first image
							if (File.Exists(lion))
							{
								using (Image img1 = Image.FromFile(lion))
								{
									g.DrawImage(img1, new Rectangle(ox - size / 2, oy - size / 2, size, size));
								}
							}
							else
							{
								using (Image img3 = Image.FromFile(error))
								{
									g.DrawImage(img3, new Rectangle(ox - size / 2, oy - size / 2, size, size));
								}
							}

							// Draw second image (next to the first image)
							if (File.Exists(forest))
							{
								using (Image img2 = Image.FromFile(forest))
								{
									g.DrawImage(img2, new Rectangle(ox + size, oy - size / 2, size, size));
								}
							}
							else
							{
								using (Image img3 = Image.FromFile(error))
								{
									g.DrawImage(img3, new Rectangle(ox - size / 2, oy - size / 2, size, size));
								}
							}
						}
						catch (Exception ex)
						{
							Console.WriteLine($"Error loading images: {ex.Message}");
						}
					}
					else if (tobj.SymbolID == 8)
					{
						string lion = Path.Combine(Environment.CurrentDirectory, "cat.jpg");
						string forest = Path.Combine(Environment.CurrentDirectory, "house.jpg");
						string error = Path.Combine(Environment.CurrentDirectory, "error.png");

						try
						{
							// Draw first image
							if (File.Exists(lion))
							{
								using (Image img1 = Image.FromFile(lion))
								{
									g.DrawImage(img1, new Rectangle(ox - size / 2, oy - size / 2, size, size));
								}
							}
							else
							{
								using (Image img3 = Image.FromFile(error))
								{
									g.DrawImage(img3, new Rectangle(ox - size / 2, oy - size / 2, size, size));
								}
							}

							// Draw second image (next to the first image)
							if (File.Exists(forest))
							{
								using (Image img2 = Image.FromFile(forest))
								{
									g.DrawImage(img2, new Rectangle(ox + size, oy - size / 2, size, size));
								}
							}
							else
							{
								using (Image img3 = Image.FromFile(error))
								{
									g.DrawImage(img3, new Rectangle(ox - size / 2, oy - size / 2, size, size));
								}
							}
						}
						catch (Exception ex)
						{
							Console.WriteLine($"Error loading images: {ex.Message}");
						}
					}
					else if (tobj.SymbolID == 9)
					{
						string lion = Path.Combine(Environment.CurrentDirectory, "aakrab.png");
						string forest = Path.Combine(Environment.CurrentDirectory, "desert.jpg");
						string error = Path.Combine(Environment.CurrentDirectory, "error.png");

						try
						{
							// Draw first image
							if (File.Exists(lion))
							{
								using (Image img1 = Image.FromFile(lion))
								{
									g.DrawImage(img1, new Rectangle(ox - size / 2, oy - size / 2, size, size));
								}
							}
							else
							{
								using (Image img3 = Image.FromFile(error))
								{
									g.DrawImage(img3, new Rectangle(ox - size / 2, oy - size / 2, size, size));
								}
							}

							// Draw second image (next to the first image)
							if (File.Exists(forest))
							{
								using (Image img2 = Image.FromFile(forest))
								{
									g.DrawImage(img2, new Rectangle(ox + size, oy - size / 2, size, size));
								}
							}
							else
							{
								using (Image img3 = Image.FromFile(error))
								{
									g.DrawImage(img3, new Rectangle(ox - size / 2, oy - size / 2, size, size));
								}
							}
						}
						catch (Exception ex)
						{
							Console.WriteLine($"Error loading images: {ex.Message}");
						}
					}
					else if (tobj.SymbolID == 10)
					{
						string lion = Path.Combine(Environment.CurrentDirectory, "snake.jpg");
						string forest = Path.Combine(Environment.CurrentDirectory, "desert.jpg");
						string error = Path.Combine(Environment.CurrentDirectory, "error.png");

						try
						{
							// Draw first image
							if (File.Exists(lion))
							{
								using (Image img1 = Image.FromFile(lion))
								{
									g.DrawImage(img1, new Rectangle(ox - size / 2, oy - size / 2, size, size));
								}
							}
							else
							{
								using (Image img3 = Image.FromFile(error))
								{
									g.DrawImage(img3, new Rectangle(ox - size / 2, oy - size / 2, size, size));
								}
							}

							// Draw second image (next to the first image)
							if (File.Exists(forest))
							{
								using (Image img2 = Image.FromFile(forest))
								{
									g.DrawImage(img2, new Rectangle(ox + size, oy - size / 2, size, size));
								}
							}
							else
							{
								using (Image img3 = Image.FromFile(error))
								{
									g.DrawImage(img3, new Rectangle(ox - size / 2, oy - size / 2, size, size));
								}
							}
						}
						catch (Exception ex)
						{
							Console.WriteLine($"Error loading images:");
						}
					}
					else if (tobj.SymbolID == 11)
					{
						string lion = Path.Combine(Environment.CurrentDirectory, "camel.jpg");
						string forest = Path.Combine(Environment.CurrentDirectory, "desert.jpg");
						string error = Path.Combine(Environment.CurrentDirectory, "error.png");

						try
						{
							// Draw first image
							if (File.Exists(lion))
							{
								using (Image img1 = Image.FromFile(lion))
								{
									g.DrawImage(img1, new Rectangle(ox - size / 2, oy - size / 2, size, size));
								}
							}
							else
							{
								using (Image img3 = Image.FromFile(error))
								{
									g.DrawImage(img3, new Rectangle(ox - size / 2, oy - size / 2, size, size));
								}
							}

							// Draw second image (next to the first image)
							if (File.Exists(forest))
							{
								using (Image img2 = Image.FromFile(forest))
								{
									g.DrawImage(img2, new Rectangle(ox + size, oy - size / 2, size, size));
								}
							}
							else
							{
								using (Image img3 = Image.FromFile(error))
								{
									g.DrawImage(img3, new Rectangle(ox - size / 2, oy - size / 2, size, size));
								}
							}
						}
						catch (Exception ex)
						{
							Console.WriteLine($"Error loading images: {ex.Message}");
						}
					}
				else
					{
						string lion = Path.Combine(Environment.CurrentDirectory, "error.png");
						try
						{
							// Draw first image
							if (File.Exists(lion))
							{
								using (Image img1 = Image.FromFile(lion))
								{
									g.DrawImage(img1, new Rectangle(ox - size / 2, oy - size / 2, size, size));
								}
							}
							else
							{
								Console.WriteLine($"Image1 not found: {lion}");
							}

							// Draw second image (next to the first image)
							
						}
						catch (Exception ex)
						{
							Console.WriteLine($"Error loading images: {ex.Message}");
						}
					}
				}
			}
		}

		// draw the blobs as usual
		// (existing blob drawing code...)
	}

	private static void Main(string[] argv)
	{
		int port = 0;
		switch (argv.Length)
		{
			case 1:
				port = int.Parse(argv[0], null);
				if (port == 0) goto default;
				break;
			case 0:
				port = 3333;
				break;
			default:
				Console.WriteLine("usage: mono TuioDemo [port]");
				System.Environment.Exit(0);
				break;
		}

		TuioDemo app = new TuioDemo(port);
		Application.Run(app);
	}
}
