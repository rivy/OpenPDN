/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) Rick Brewster, Tom Jackson, and past contributors.            //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Setup/License.txt for full licensing and attribution details.       //
// 2                                                                           //
// 1                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.CodeDom.Compiler;

namespace PaintDotNet.Effects
{
	/// <summary>
	/// Container for a CompilerError object, overrides ToString to a more readable form.
	/// </summary>
	public class CompilerErrorWrapper
	{
		public CompilerError CompilerError = null;

		public override string ToString()
		{
			if (this.CompilerError == null) 
			{
				throw new ArgumentNullException("inner", "inner may not be null");
			}

			return (this.CompilerError.IsWarning  ? "Warning" : "Error")
				+ " at line "
				+ this.CompilerError.Line
				+ ": "
				+ this.CompilerError.ErrorText
				+ " (" + this.CompilerError.ErrorNumber + ")";
		}

	}
}
