// Code from: https://github.com/dotnet/winforms/blob/100762af36a2d0caf6adc1e46f6d6b9ca1e6a8c0/src/System.Windows.Forms/src/System/Windows/Forms/CheckState.cs

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Aigamo.Saruhashi;

/// <summary>
///  Specifies the state of a control,  such as a check box, that can be
///  checked, unchecked, or set to an indeterminate state.
/// </summary>
public enum CheckState
{
    /// <summary>
    ///  The control is unchecked.
    /// </summary>
    Unchecked = 0,

    /// <summary>
    ///  The control is checked.
    /// </summary>
    Checked = 1,

    /// <summary>
    ///  The control is indeterminate. An indeterminate control generally has
    ///  a shaded appearance.
    /// </summary>
    Indeterminate = 2,
}
