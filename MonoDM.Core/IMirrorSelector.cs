using System;
using System.Collections.Generic;
using System.Text;

namespace MonoDM.Core
{
    public interface IMirrorSelector
    {
        void Init(Downloader downloader);

        ResourceLocation GetNextResourceLocation(); 
    }
}
