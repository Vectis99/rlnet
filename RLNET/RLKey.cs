#region license
/* 
 * Released under the MIT License (MIT)
 * Copyright (c) 2014 Travis M. Clark
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to 
 * deal in the Software without restriction, including without limitation the 
 * rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
 * sell copies of the Software, and to permit persons to whom the Software is 
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in 
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Input;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace RLNET
{
    /// <summary>
    /// Wrapper for <see cref="OpenTK.Windowing.GraphicsLibraryFramework"/>
    /// maintained for compatability.
    /// </summary>
    public enum RLKey 
    {
        // Summary:
        //     A key outside the known keys.
        Unknown = Keys.Unknown,
        //
        // Summary:
        //     The left shift key (equivalent to ShiftLeft).
        LShift = Keys.LeftShift,
        //
        // Summary:
        //     The left shift key.
        ShiftLeft = Keys.LeftShift,
        //
        // Summary:
        //     The right shift key (equivalent to ShiftRight).
        RShift = Keys.RightShift,
        //
        // Summary:
        //     The right shift key.
        ShiftRight = Keys.RightShift,
        //
        // Summary:
        //     The left control key (equivalent to ControlLeft).
        LControl = Keys.LeftControl,
        //
        // Summary:
        //     The left control key.
        ControlLeft = Keys.LeftControl,
        //
        // Summary:
        //     The right control key (equivalent to ControlRight).
        RControl = Keys.RightControl,
        //
        // Summary:
        //     The right control key.
        ControlRight = Keys.RightControl,
        //
        // Summary:
        //     The left alt key.
        AltLeft = Keys.LeftAlt,
        //
        // Summary:
        //     The left alt key (equivalent to AltLeft.
        LAlt = Keys.LeftAlt,
        //
        // Summary:
        //     The right alt key.
        AltRight = Keys.RightAlt,
        //
        // Summary:
        //     The right alt key (equivalent to AltRight).
        RAlt = Keys.RightAlt,
        //
        // Summary:
        //     The left win key.
        WinLeft = Keys.LeftSuper,
        //
        // Summary:
        //     The left win key (equivalent to WinLeft).
        LWin = Keys.LeftSuper,
        //
        // Summary:
        //     The right win key (equivalent to WinRight).
        RWin = Keys.RightSuper,
        //
        // Summary:
        //     The right win key.
        WinRight = Keys.RightSuper,
        //
        // Summary:
        //     The menu key.
        Menu = Keys.Menu,
        //
        // Summary:
        //     The F1 key.
        F1 = Keys.F1,
        //
        // Summary:
        //     The F2 key.
        F2 = Keys.F2,
        //
        // Summary:
        //     The F3 key.
        F3 = Keys.F3,
        //
        // Summary:
        //     The F4 key.
        F4 = Keys.F4,
        //
        // Summary:
        //     The F5 key.
        F5 = Keys.F5,
        //
        // Summary:
        //     The F6 key.
        F6 = Keys.F6,
        //
        // Summary:
        //     The F7 key.
        F7 = Keys.F7,
        //
        // Summary:
        //     The F8 key.
        F8 = Keys.F8,
        //
        // Summary:
        //     The F9 key.
        F9 = Keys.F9,
        //
        // Summary:
        //     The F10 key.
        F10 = Keys.F10,
        //
        // Summary:
        //     The F11 key.
        F11 = Keys.F11,
        //
        // Summary:
        //     The F12 key.
        F12 = Keys.F12,
        //
        // Summary:
        //     The F13 key.
        F13 = Keys.F13,
        //
        // Summary:
        //     The F14 key.
        F14 = Keys.F14,
        //
        // Summary:
        //     The F15 key.
        F15 = Keys.F15,
        //
        // Summary:
        //     The F16 key.
        F16 = Keys.F16,
        //
        // Summary:
        //     The F17 key.
        F17 = Keys.F17,
        //
        // Summary:
        //     The F18 key.
        F18 = Keys.F18,
        //
        // Summary:
        //     The F19 key.
        F19 = Keys.F19,
        //
        // Summary:
        //     The F20 key.
        F20 = Keys.F20,
        //
        // Summary:
        //     The F21 key.
        F21 = Keys.F21,
        //
        // Summary:
        //     The F22 key.
        F22 = Keys.F22,
        //
        // Summary:
        //     The F23 key.
        F23 = Keys.F23,
        //
        // Summary:
        //     The F24 key.
        F24 = Keys.F24,
        //
        // Summary:
        //     The F25 key.
        F25 = Keys.F25,
        //
        // Summary:
        //     The F26 key.
        F26,
        //
        // Summary:
        //     The F27 key.
        F27,
        //
        // Summary:
        //     The F28 key.
        F28,
        //
        // Summary:
        //     The F29 key.
        F29,
        //
        // Summary:
        //     The F30 key.
        F30,
        //
        // Summary:
        //     The F31 key.
        F31,
        //
        // Summary:
        //     The F32 key.
        F32,
        //
        // Summary:
        //     The F33 key.
        F33,
        //
        // Summary:
        //     The F34 key.
        F34,
        //
        // Summary:
        //     The F35 key.
        F35,
        //
        // Summary:
        //     The up arrow key.
        Up = Keys.Up,
        //
        // Summary:
        //     The down arrow key.
        Down = Keys.Down,
        //
        // Summary:
        //     The left arrow key.
        Left = Keys.Left,
        //
        // Summary:
        //     The right arrow key.
        Right = Keys.Right,
        //
        // Summary:
        //     The enter key.
        Enter = Keys.Enter,
        //
        // Summary:
        //     The escape key.
        Escape = Keys.Escape,
        //
        // Summary:
        //     The space key.
        Space = Keys.Space,
        //
        // Summary:
        //     The tab key.
        Tab = Keys.Tab,
        //
        // Summary:
        //     The backspace key (equivalent to BackSpace).
        Back = Keys.Backspace,
        //
        // Summary:
        //     The backspace key.
        BackSpace = Keys.Backspace,
        //
        // Summary:
        //     The insert key.
        Insert = Keys.Insert,
        //
        // Summary:
        //     The delete key.
        Delete = Keys.Delete,
        //
        // Summary:
        //     The page up key.
        PageUp = Keys.PageUp,
        //
        // Summary:
        //     The page down key.
        PageDown = Keys.PageDown,
        //
        // Summary:
        //     The home key.
        Home = Keys.Home,
        //
        // Summary:
        //     The end key.
        End = Keys.End,
        //
        // Summary:
        //     The caps lock key.
        CapsLock = Keys.CapsLock,
        //
        // Summary:
        //     The scroll lock key.
        ScrollLock = Keys.ScrollLock,
        //
        // Summary:
        //     The print screen key.
        PrintScreen = Keys.PrintScreen,
        //
        // Summary:
        //     The pause key.
        Pause = Keys.Pause,
        //
        // Summary:
        //     The num lock key.
        NumLock = Keys.NumLock,
        //
        // Summary:
        //     The clear key (Keypad5 with NumLock disabled, on typical keyboards).
        Clear = Keys.KeyPad5,
        //
        // Summary:
        //     The sleep key.
        Sleep, // ???
        //
        // Summary:
        //     The keypad 0 key.
        Keypad0 = Keys.KeyPad0,
        //
        // Summary:
        //     The keypad 1 key.
        Keypad1 = Keys.KeyPad1,
        //
        // Summary:
        //     The keypad 2 key.
        Keypad2 = Keys.KeyPad2,
        //
        // Summary:
        //     The keypad 3 key.
        Keypad3 = Keys.KeyPad3,
        //
        // Summary:
        //     The keypad 4 key.
        Keypad4 = Keys.KeyPad4,
        //
        // Summary:
        //     The keypad 5 key.
        Keypad5 = Keys.KeyPad5,
        //
        // Summary:
        //     The keypad 6 key.
        Keypad6 = Keys.KeyPad6,
        //
        // Summary:
        //     The keypad 7 key.
        Keypad7 = Keys.KeyPad7,
        //
        // Summary:
        //     The keypad 8 key.
        Keypad8 = Keys.KeyPad8,
        //
        // Summary:
        //     The keypad 9 key.
        Keypad9 = Keys.KeyPad9,
        //
        // Summary:
        //     The keypad divide key.
        KeypadDivide = Keys.KeyPadDivide,
        //
        // Summary:
        //     The keypad multiply key.
        KeypadMultiply = Keys.KeyPadMultiply,
        //
        // Summary:
        //     The keypad subtract key.
        KeypadSubtract = Keys.KeyPadSubtract,
        //
        // Summary:
        //     The keypad minus key (equivalent to KeypadSubtract).
        KeypadMinus = Keys.KeyPadSubtract,
        //
        // Summary:
        //     The keypad plus key (equivalent to KeypadAdd).
        KeypadPlus = Keys.KeyPadAdd,
        //
        // Summary:
        //     The keypad add key.
        KeypadAdd = Keys.KeyPadAdd,
        //
        // Summary:
        //     The keypad decimal key.
        KeypadDecimal = Keys.KeyPadDecimal,
        //
        // Summary:
        //     The keypad period key (equivalent to KeypadDecimal).
        KeypadPeriod = Keys.KeyPadDecimal,
        //
        // Summary:
        //     The keypad enter key.
        KeypadEnter = Keys.KeyPadEnter,
        //
        // Summary:
        //     The A key.
        A = Keys.A,
        //
        // Summary:
        //     The B key.
        B = Keys.B,
        //
        // Summary:
        //     The C key.
        C = Keys.C,
        //
        // Summary:
        //     The D key.
        D = Keys.D,
        //
        // Summary:
        //     The E key.
        E = Keys.E,
        //
        // Summary:
        //     The F key.
        F = Keys.F,
        //
        // Summary:
        //     The G key.
        G = Keys.G,
        //
        // Summary:
        //     The H key.
        H = Keys.H,
        //
        // Summary:
        //     The I key.
        I = Keys.I,
        //
        // Summary:
        //     The J key.
        J = Keys.J,
        //
        // Summary:
        //     The K key.
        K = Keys.K,
        //
        // Summary:
        //     The L key.
        L = Keys.L,
        //
        // Summary:
        //     The M key.
        M = Keys.M,
        //
        // Summary:
        //     The N key.
        N = Keys.N,
        //
        // Summary:
        //     The O key.
        O = Keys.O,
        //
        // Summary:
        //     The P key.
        P = Keys.P,
        //
        // Summary:
        //     The Q key.
        Q = Keys.Q,
        //
        // Summary:
        //     The R key.
        R = Keys.R,
        //
        // Summary:
        //     The S key.
        S = Keys.S,
        //
        // Summary:
        //     The T key.
        T = Keys.T,
        //
        // Summary:
        //     The U key.
        U = Keys.U,
        //
        // Summary:
        //     The V key.
        V = Keys.V,
        //
        // Summary:
        //     The W key.
        W = Keys.W,
        //
        // Summary:
        //     The X key.
        X = Keys.X,
        //
        // Summary:
        //     The Y key.
        Y = Keys.Y,
        //
        // Summary:
        //     The Z key.
        Z = Keys.Z,
        //
        // Summary:
        //     The number 0 key.
        Number0 = Keys.D0,
        //
        // Summary:
        //     The number 1 key.
        Number1 = Keys.D1,
        //
        // Summary:
        //     The number 2 key.
        Number2 = Keys.D2,
        //
        // Summary:
        //     The number 3 key.
        Number3 = Keys.D3,
        //
        // Summary:
        //     The number 4 key.
        Number4 = Keys.D4,
        //
        // Summary:
        //     The number 5 key.
        Number5 = Keys.D5,
        //
        // Summary:
        //     The number 6 key.
        Number6 = Keys.D6,
        //
        // Summary:
        //     The number 7 key.
        Number7 = Keys.D7,
        //
        // Summary:
        //     The number 8 key.
        Number8 = Keys.D8,
        //
        // Summary:
        //     The number 9 key.
        Number9 = Keys.D9,
        //
        // Summary:
        //     The grave key (equivaent to Tilde).
        Grave = Keys.GraveAccent,
        //
        // Summary:
        //     The tilde key.
        Tilde = Keys.GraveAccent,
        //
        // Summary:
        //     The minus key.
        Minus = Keys.Minus,
        //
        // Summary:
        //     The plus key.
        Plus = Keys.Equal,
        //
        // Summary:
        //     The left bracket key (equivalent to BracketLeft).
        LBracket = Keys.LeftBracket,
        //
        // Summary:
        //     The left bracket key.
        BracketLeft = Keys.LeftBracket,
        //
        // Summary:
        //     The right bracket key (equivalent to BracketRight).
        RBracket = Keys.RightBracket,
        //
        // Summary:
        //     The right bracket key.
        BracketRight = Keys.RightBracket,
        //
        // Summary:
        //     The semicolon key.
        Semicolon = Keys.Semicolon,
        //
        // Summary:
        //     The quote key.
        Quote = Keys.Apostrophe,
        //
        // Summary:
        //     The comma key.
        Comma = Keys.Comma,
        //
        // Summary:
        //     The period key.
        Period = Keys.Period,
        //
        // Summary:
        //     The slash key.
        Slash = Keys.Slash,
        //
        // Summary:
        //     The backslash key.
        BackSlash = Keys.Backslash,
        //
        // Summary:
        //     The secondary backslash key.
        NonUSBackSlash,
        //
        // Summary:
        //     Indicates the last available keyboard key.
        LastKey = Keys.LastKey
    }
}
