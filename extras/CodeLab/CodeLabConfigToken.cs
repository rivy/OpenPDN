/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) Rick Brewster, Tom Jackson, and past contributors.            //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Setup/License.txt for full licensing and attribution details.       //
// 2                                                                           //
// 1                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace PaintDotNet.Effects
{
	public class CodeLabConfigToken : EffectConfigToken
	{
		public Effect UserScriptObject;
		public string UserCode;
		public List<Exception> LastExceptions;
		public string ScriptName;

		public CodeLabConfigToken() : base()
		{
			UserScriptObject = null;
			UserCode = "";
            LastExceptions = new List<Exception>();
			ScriptName = "MyScript";
		}

		public override object Clone()
		{
			CodeLabConfigToken sect = new CodeLabConfigToken();
			sect.UserCode = this.UserCode;
			sect.UserScriptObject = this.UserScriptObject;
            sect.LastExceptions = this.LastExceptions; //Reference copy INTENDED.
			sect.ScriptName = this.ScriptName;
			return sect;
		}
	}
}
