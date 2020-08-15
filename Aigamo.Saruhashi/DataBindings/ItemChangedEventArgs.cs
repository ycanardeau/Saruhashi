﻿// Code from: https://github.com/dotnet/winforms/blob/100762af36a2d0caf6adc1e46f6d6b9ca1e6a8c0/src/System.Windows.Forms/src/System/Windows/Forms/ItemChangedEventArgs.cs

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Aigamo.Saruhashi
{
    public class ItemChangedEventArgs : EventArgs
    {
        internal ItemChangedEventArgs(int index)
        {
            Index = index;
        }

        public int Index { get; }
    }
}
