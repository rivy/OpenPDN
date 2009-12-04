/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) Rick Brewster, Tom Jackson, and past contributors.            //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Setup/License.txt for full licensing and attribution details.       //
// 2                                                                           //
// 1                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Windows.Forms;

namespace PaintDotNet.Effects
{
	public class CodeEditor : TextBox
	{
        private Timer compileTimer;

		public CodeEditor()
		{
			this.AcceptsReturn = true;
			this.AcceptsTab = true;		
			this.Multiline = true;
			this.ScrollBars = ScrollBars.Vertical;
			this.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));

            this.compileTimer = new Timer();
            this.compileTimer.Interval = 500;
            this.compileTimer.Enabled = false;
            this.compileTimer.Tick += new EventHandler(compileTimer_Tick);
		}

        public event EventHandler CompileTimeHint;
        protected virtual void OnCompileTimeHint()
        {
            if (CompileTimeHint != null)
            {
                CompileTimeHint(this, EventArgs.Empty);
            }
        }

		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			if (e.KeyChar == '\r')
			{
				int count = 0;
				int end = this.SelectionStart;
				string text = this.Text, indent = "\r\n";
				for (int i = 0; i < end; i++) 
				{
					if (text[i] == '(' || text[i] == '[' || text[i] == '{') 
					{
						count++;
					}
					if (text[i] == ')' || text[i] == ']' || text[i] == '}') 
					{
						count--;
					}
				}

				while (count-- > 0)
				{
					indent += "    ";
				}

				this.SelectedText = indent;
				e.Handled = true;
			}

            this.compileTimer.Enabled = false;
            this.compileTimer.Enabled = true;

			base.OnKeyPress (e);
		}

		public void Highlight(int line, int column) 
		{
            int startIndex = 0;
            int endIndex = -1;
            int linesPassed = 0;
            string txt = this.Text;

            for (int i = 0; i < txt.Length; ++i)
            {
                if (txt[i] == '\n')
                {
                    linesPassed++;

                    if (linesPassed == line - 1)
                    {
                        startIndex = i + column;
                    }
                    else if (linesPassed == line)
                    {
                        endIndex = i - 1;
                    }
                }
            }

            if (startIndex > 0 && endIndex > 0)
            {
                this.Select(startIndex, endIndex - startIndex);
            }
        }

        private void compileTimer_Tick(object sender, EventArgs e)
        {
            OnCompileTimeHint();
            this.compileTimer.Enabled = false;
        }
    }
}
