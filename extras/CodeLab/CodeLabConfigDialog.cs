/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) Rick Brewster, Tom Jackson, and past contributors.            //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Setup/License.txt for full licensing and attribution details.       //
// 2                                                                           //
// 1                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using System.Windows.Forms;
using Microsoft.CSharp;

namespace PaintDotNet.Effects
{
    public class CodeLabConfigDialog : EffectConfigDialog
    {
        private CodeEditor txtCode;
        private System.Windows.Forms.GroupBox grpCode;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnBuild;
        private System.Windows.Forms.ListBox listErrors;
        private CompilerParameters param;
        private CompilerResults result;
        private Assembly userAssembly;
        private System.Windows.Forms.ToolTip toolTips;
        private System.ComponentModel.IContainer components;
        private Effect userScriptObject;
        private const int prependLines = 13;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Timer tmrExceptionCheck;
        private System.Windows.Forms.Label lblScriptName;
        private System.Windows.Forms.Button btnBuildDLL;
        private System.Windows.Forms.TextBox txtScriptName;
        private int lineOffset;
        private const string prepend = ""
            + "using System;\n"
            + "using System.Drawing;\n"
            + "using PaintDotNet;\n"
            + "using PaintDotNet.Effects;\n"
            + "namespace PaintDotNet.Effects\n"
            + "{\n"
            + "[EffectCategory(EffectCategory.Effect)]\n"
            + "public class UserScript : Effect\n"
            + "{\n"
            + "public override void Render(EffectConfigToken parameters, RenderArgs dstArgs, RenderArgs srcArgs, Rectangle[] rois, int startIndex, int length)"
            + "{"
            + "for (int i = startIndex; i < startIndex + length; ++i)"
            + "{"
            + "Render(dstArgs.Surface, srcArgs.Surface, rois[i]);\n"
            + "}\n"
            + "}\n"
            + "public UserScript()";
        private const string append = "}\n}";
        private CSharpCodeProvider cscp = new CSharpCodeProvider();

        public CodeLabConfigDialog()
        {
            param = new CompilerParameters();
            param.GenerateInMemory = true;
            param.IncludeDebugInformation = true;
            param.GenerateExecutable = false;
            param.GenerateInMemory = true;
            param.CompilerOptions = param.CompilerOptions + " /unsafe /optimize";

            string basePath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().CodeBase.Substring(8));

            //			param.OutputAssembly = "UserScript";
            param.ReferencedAssemblies.Add("System.dll");
            param.ReferencedAssemblies.Add("System.Drawing.dll");
            param.ReferencedAssemblies.Add("System.Windows.Forms.dll");
            param.ReferencedAssemblies.Add(Path.Combine(basePath, "PdnLib.dll"));
            param.ReferencedAssemblies.Add(Path.Combine(basePath, "PaintDotNet.Effects.dll"));
            param.ReferencedAssemblies.Add(Path.Combine(basePath, "PaintDotNet.Data.dll"));
            param.ReferencedAssemblies.Add(Path.Combine(basePath, "PaintDotNet.SystemLayer.dll"));
            param.ReferencedAssemblies.Add(Path.Combine(basePath, "Effects\\CodeLab.dll"));

            userAssembly = null;

            InitializeComponent();

            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.AutoScaleMode = AutoScaleMode.Dpi;

            ResetScript();

            lineOffset = CountOccurances(prepend, '\n');

            btnBuild_Click(null, EventArgs.Empty);
        }

        private int CountOccurances(string str, char compare)
        {
            int count = 0;

            foreach (char c in str)
            {
                if (c == compare)
                {
                    ++count;
                }
            }

            return count;
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.txtCode = new PaintDotNet.Effects.CodeEditor();
            this.grpCode = new System.Windows.Forms.GroupBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnBuild = new System.Windows.Forms.Button();
            this.listErrors = new System.Windows.Forms.ListBox();
            this.toolTips = new System.Windows.Forms.ToolTip(this.components);
            this.btnLoad = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.tmrExceptionCheck = new System.Windows.Forms.Timer(this.components);
            this.btnBuildDLL = new System.Windows.Forms.Button();
            this.txtScriptName = new System.Windows.Forms.TextBox();
            this.lblScriptName = new System.Windows.Forms.Label();
            this.grpCode.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtCode
            // 
            this.txtCode.AcceptsReturn = true;
            this.txtCode.AcceptsTab = true;
            this.txtCode.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCode.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCode.HideSelection = false;
            this.txtCode.Location = new System.Drawing.Point(8, 16);
            this.txtCode.Multiline = true;
            this.txtCode.Name = "txtCode";
            this.txtCode.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtCode.Size = new System.Drawing.Size(510, 262);
            this.txtCode.TabIndex = 0;
            this.txtCode.CompileTimeHint += new System.EventHandler(this.txtCode_CompileTimeHint);
            // 
            // grpCode
            // 
            this.grpCode.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.grpCode.Controls.Add(this.txtCode);
            this.grpCode.Location = new System.Drawing.Point(8, 32);
            this.grpCode.Name = "grpCode";
            this.grpCode.Size = new System.Drawing.Size(528, 286);
            this.grpCode.TabIndex = 2;
            this.grpCode.TabStop = false;
            this.grpCode.Text = "C# Code:";
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(473, 326);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(64, 24);
            this.btnOK.TabIndex = 7;
            this.btnOK.Text = "&OK";
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(473, 358);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(64, 24);
            this.btnCancel.TabIndex = 8;
            this.btnCancel.Text = "&Cancel";
            // 
            // btnBuild
            // 
            this.btnBuild.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBuild.Location = new System.Drawing.Point(473, 8);
            this.btnBuild.Name = "btnBuild";
            this.btnBuild.Size = new System.Drawing.Size(64, 23);
            this.btnBuild.TabIndex = 6;
            this.btnBuild.Text = "&Build";
            this.btnBuild.Click += new System.EventHandler(this.btnBuild_Click);
            // 
            // listErrors
            // 
            this.listErrors.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listErrors.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listErrors.ItemHeight = 16;
            this.listErrors.Items.AddRange(new object[] {
            "Click \'Build\'"});
            this.listErrors.Location = new System.Drawing.Point(8, 328);
            this.listErrors.Name = "listErrors";
            this.listErrors.Size = new System.Drawing.Size(456, 52);
            this.listErrors.TabIndex = 0;
            this.listErrors.SelectedIndexChanged += new System.EventHandler(this.listErrors_SelectedIndexChanged);
            // 
            // btnLoad
            // 
            this.btnLoad.Location = new System.Drawing.Point(232, 8);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(80, 23);
            this.btnLoad.TabIndex = 3;
            this.btnLoad.Text = "&Load Source";
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(144, 8);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(80, 23);
            this.btnSave.TabIndex = 2;
            this.btnSave.Text = "&Save Source";
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(320, 8);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(48, 23);
            this.btnClear.TabIndex = 4;
            this.btnClear.Text = "C&lear";
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // tmrExceptionCheck
            // 
            this.tmrExceptionCheck.Enabled = true;
            this.tmrExceptionCheck.Tick += new System.EventHandler(this.tmrExceptionCheck_Tick);
            // 
            // btnBuildDLL
            // 
            this.btnBuildDLL.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBuildDLL.Location = new System.Drawing.Point(393, 8);
            this.btnBuildDLL.Name = "btnBuildDLL";
            this.btnBuildDLL.Size = new System.Drawing.Size(72, 23);
            this.btnBuildDLL.TabIndex = 5;
            this.btnBuildDLL.Text = "Make &DLL";
            this.btnBuildDLL.Click += new System.EventHandler(this.btnSaveDLL_Click);
            // 
            // txtScriptName
            // 
            this.txtScriptName.Location = new System.Drawing.Point(48, 8);
            this.txtScriptName.Name = "txtScriptName";
            this.txtScriptName.Size = new System.Drawing.Size(88, 20);
            this.txtScriptName.TabIndex = 1;
            this.txtScriptName.Text = "MyCode";
            // 
            // lblScriptName
            // 
            this.lblScriptName.Location = new System.Drawing.Point(8, 8);
            this.lblScriptName.Name = "lblScriptName";
            this.lblScriptName.Size = new System.Drawing.Size(40, 20);
            this.lblScriptName.TabIndex = 8;
            this.lblScriptName.Text = "Name:";
            this.lblScriptName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // CodeLabConfigDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.ClientSize = new System.Drawing.Size(544, 389);
            this.Controls.Add(this.lblScriptName);
            this.Controls.Add(this.txtScriptName);
            this.Controls.Add(this.btnBuildDLL);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnLoad);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.grpCode);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnBuild);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.listErrors);
            this.MinimumSize = new System.Drawing.Size(535, 200);
            this.AcceptButton = this.btnOK;
            this.CancelButton = this.btnCancel;
            this.Name = "CodeLabConfigDialog";
            this.Text = "Code Lab (Alpha)";
            this.Controls.SetChildIndex(this.listErrors, 0);
            this.Controls.SetChildIndex(this.btnClear, 0);
            this.Controls.SetChildIndex(this.btnBuild, 0);
            this.Controls.SetChildIndex(this.btnCancel, 0);
            this.Controls.SetChildIndex(this.grpCode, 0);
            this.Controls.SetChildIndex(this.btnOK, 0);
            this.Controls.SetChildIndex(this.btnLoad, 0);
            this.Controls.SetChildIndex(this.btnSave, 0);
            this.Controls.SetChildIndex(this.btnBuildDLL, 0);
            this.Controls.SetChildIndex(this.txtScriptName, 0);
            this.Controls.SetChildIndex(this.lblScriptName, 0);
            this.grpCode.ResumeLayout(false);
            this.grpCode.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        private void listErrors_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (listErrors.SelectedIndex >= 0)
            {
                CompilerErrorWrapper errw = listErrors.SelectedItem as CompilerErrorWrapper;
                if (errw != null)
                {
                    txtCode.Highlight(errw.CompilerError.Line - lineOffset, errw.CompilerError.Column);
                }
                toolTips.SetToolTip(listErrors, listErrors.SelectedItem.ToString());
            }
        }

        private void btnBuild_Click(object sender, System.EventArgs e)
        {
            Build(false);
        }

        protected override void InitTokenFromDialog()
        {
            CodeLabConfigToken sect = (CodeLabConfigToken)theEffectToken;
            sect.UserCode = txtCode.Text;
            sect.UserScriptObject = this.userScriptObject;
            sect.ScriptName = txtScriptName.Text;
        }

        protected override void InitDialogFromToken(EffectConfigToken effectToken)
        {
            CodeLabConfigToken sect = (CodeLabConfigToken)effectToken;

            txtScriptName.Text = sect.ScriptName;
            if (sect != null)
            {
                txtCode.Text = sect.UserCode;
            }
        }

        protected override void InitialInitToken()
        {
            CodeLabConfigToken sect = new CodeLabConfigToken();
            sect.UserCode = "void Render(Surface dst, Surface src, Rectangle rect)\r\n{\r\n    for(int y = rect.Top; y < rect.Bottom; y++)\r\n    {\r\n        for (int x = rect.Left; x < rect.Right; x++)\r\n        {\r\n        }\r\n    }\r\n}";
            sect.UserScriptObject = null;
            sect.ScriptName = "MyScript";
            theEffectToken = sect;
        }

        private bool Build(bool toDll)
        {
            bool retVal = false;
            listErrors.Items.Clear();
            try
            {
                string prepend2 = " : base(\"" + txtScriptName.Text + "\", null) {}";
                if (toDll)
                {
                    string oldargs = param.CompilerOptions;

                    Uri location = new Uri(System.Reflection.Assembly.GetEntryAssembly().CodeBase);
                    string fullPath = Uri.UnescapeDataString(System.IO.Path.GetDirectoryName(location.AbsolutePath));

                    fullPath = Path.Combine(fullPath, "Effects");
                    fullPath = Path.Combine(fullPath, txtScriptName.Text);
                    fullPath = Path.ChangeExtension(fullPath, ".dll");

                    param.CompilerOptions = param.CompilerOptions + " /debug- /target:library /out:\"" + fullPath + "\"";
                    cscp.CompileAssemblyFromSource(param, prepend + prepend2 + txtCode.Text + append);

                    param.CompilerOptions = oldargs;

                }
                else
                {
                    userScriptObject = null;
                    result = cscp.CompileAssemblyFromSource(param, prepend + prepend2 + txtCode.Text + append);
                }

                if (result.Errors.HasErrors)
                {
                    foreach (CompilerError err in result.Errors)
                    {
                        CompilerErrorWrapper cew = new CompilerErrorWrapper();
                        cew.CompilerError = err;
                        listErrors.Items.Add(cew);
                    }
                }
                else if (!toDll)
                {
                    userAssembly = result.CompiledAssembly;

                    foreach (Type type in userAssembly.GetTypes())
                    {
                        if (type.IsSubclassOf(typeof(Effect)) && !type.IsAbstract)
                        {
                            userScriptObject = (Effect)type.GetConstructor(Type.EmptyTypes).Invoke(new object[] { });
                        }
                    }
                    retVal = (userScriptObject != null);
                }
                else
                {
                    retVal = true;
                }
            }
            catch (Exception exc)
            {
                userScriptObject = null;
                listErrors.Items.Add("Internal Error: " + exc.ToString());
            }
            if (!toDll)
            {
                FinishTokenUpdate();
            }
            return retVal;
        }

        private bool ResetScript()
        {
            InitialInitToken();
            InitDialogFromToken();
            FinishTokenUpdate();
            return true;
        }

        private bool SaveScript()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Save User Script";
            sfd.DefaultExt = ".cs";
            sfd.Filter = "C# Code Files (*.CS)|*.cs";
            sfd.OverwritePrompt = true;
            sfd.AddExtension = true;
            sfd.FileName = txtScriptName.Text + ".cs";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    StreamWriter sw = new StreamWriter(sfd.FileName);

                    sw.Write(txtCode.Text);

                    sw.Close();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private bool LoadScript()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Load User Script";
            ofd.DefaultExt = ".cs";
            ofd.Filter = "C# Code Files (*.CS)|*.cs";
            ofd.DefaultExt = ".cs";
            ofd.Multiselect = false;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    StreamReader sr = new StreamReader(ofd.FileName);

                    txtCode.Text = sr.ReadToEnd();

                    sr.Close();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private void btnSave_Click(object sender, System.EventArgs e)
        {
            SaveScript();
        }

        private void btnLoad_Click(object sender, System.EventArgs e)
        {
            LoadScript();
            this.btnBuild.PerformClick();
        }

        private void btnClear_Click(object sender, System.EventArgs e)
        {
            DialogResult dr = MessageBox.Show(this, "Do you want to save your current script?", "Script Editor", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            switch (dr)
            {
                case DialogResult.Yes:
                    if (SaveScript())
                    {
                        ResetScript();
                    }
                    break;
                case DialogResult.No:
                    ResetScript();
                    break;
                case DialogResult.Cancel:
                    break;
            }
        }

        private void tmrExceptionCheck_Tick(object sender, System.EventArgs e)
        {
            CodeLabConfigToken sect = (CodeLabConfigToken)theEffectToken;

            if (sect.LastExceptions.Count > 0)
            {
                Exception exc = sect.LastExceptions[0];
                sect.LastExceptions.Clear();
                listErrors.Items.Add(exc.ToString());
            }

        }

        private void btnSaveDLL_Click(object sender, System.EventArgs e)
        {
            Build(true);
        }

        private void txtCode_CompileTimeHint(object sender, EventArgs e)
        {
            this.btnBuild.PerformClick();
        }
    }
}
