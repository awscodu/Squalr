﻿namespace Squalr.Engine.Scripting.Graphics
{
    using System;

    /// <summary>
    /// Provides functions to interact with graphics in an external process for scripts
    /// </summary>
    internal class GraphicsCore : IGraphicsCore
    {
        public Guid CreateText(String text, Int32 locationX, Int32 locationY)
        {
            throw new NotImplementedException();
        }

        public Guid CreateImage(String path, Int32 locationX, Int32 locationY)
        {
            throw new NotImplementedException();
        }

        public void DestroyObject(Guid guid)
        {
            throw new NotImplementedException();
        }

        public void ShowObject(Guid guid)
        {
            throw new NotImplementedException();
        }

        public void HideObject(Guid guid)
        {
            throw new NotImplementedException();
        }
    }
    //// End interface
}
//// End namespace