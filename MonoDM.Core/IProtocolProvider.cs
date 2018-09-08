using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace MonoDM.Core
{
    public interface IProtocolProvider
    {
        // TODO: remove this method?
        void Initialize(Downloader downloader);

        Stream CreateStream(ResourceLocation rl, long initialPosition, long endPosition);

        RemoteFileInfo GetFileInfo(ResourceLocation rl, out Stream stream);
    }
}
