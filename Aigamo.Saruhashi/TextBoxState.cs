// Code from: https://github.com/dotnet/winforms/blob/100762af36a2d0caf6adc1e46f6d6b9ca1e6a8c0/src/System.Windows.Forms/src/System/Windows/Forms/VisualStyles/TextBoxState.cs

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Aigamo.Saruhashi
{
	public enum TextBoxState
	{
		Normal = 1,
		Hot = 2,
		Selected = 3,
		Disabled = 4,
		//Focused = 5,
		Readonly = 6,
		Assist = 7
	}
}
