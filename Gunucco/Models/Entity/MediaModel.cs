using Gunucco.Common;
using Gunucco.Entities;
using Gunucco.Models.Database;
using Gunucco.Models.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Gunucco.Models.Entity
{
    public class MediaModel
    {
        private const string MediaDirPath = "/media/";

        public AuthorizationData AuthData { get; set; }

        public Content Content { get; set; }

        public Media Media { get; set; }

        public static MediaExtension StringToExtension(string str)
        {
            switch (str.ToLower())
            {
                case "png":
                    return MediaExtension.Png;
                case "jpg":
                case "jpeg":
                    return MediaExtension.Jpeg;
                case "gif":
                    return MediaExtension.Gif;
                default:
                    throw new GunuccoException(new ApiMessage
                    {
                        StatusCode = 400,
                        Message = "Invalid or unsupported extension.",
                    });
            }
        }

        private void CheckMediaDirectory()
        {
            if (!Directory.Exists(Config.MediaDirPath))
            {
                throw new GunuccoException(new ApiMessage
                {
                    StatusCode = 503,
                    Message = "Media directory is not found. Report this to server administrator.",
                });
            }
        }

        public void SaveMediaAsFile()
        {
            if (this.Content.Type == ContentType.Image && this.Media.Source == MediaSource.Self)
            {
                var fileName = $"{this.Media.Id:00000000}";
                this.Media.FilePath = MediaDirPath + fileName;
                this.CheckMediaDirectory();

                try
                {
                    FileStream fs = new FileStream(Config.MediaDirPath + fileName, FileMode.Create);
                    BinaryWriter bw = new BinaryWriter(fs);
                    bw.Write(this.Media.MediaDataRow);
                    bw.Dispose();
                    fs.Dispose();
                }
                catch (Exception e)
                {
                    throw new GunuccoException(new ApiMessage
                    {
                        StatusCode = 503,
                        Message = "Cannot write media file. Report this to server administrator.",
                    }, e);
                }
            }
        }

        public void LoadMediaFromFile()
        {
            if (this.Media.Source == MediaSource.Self)
            {
                var fileName = Config.MediaDirPath + this.Media.FilePath.Replace(MediaDirPath, "");

                var fi = new FileInfo(fileName);
                if (!fi.Exists)
                {
                    throw new GunuccoException(new ApiMessage
                    {
                        StatusCode = 404,
                        Message = "Media file is not found.",
                    });
                }
                var fileSize = (int)fi.Length;

                FileStream fs = fi.OpenRead();
                BinaryReader br = new BinaryReader(fs);
                this.Media.MediaDataRow = br.ReadBytes(fileSize);
                br.Dispose();
                fs.Dispose();
            }
        }

        public void DeleteMediaFile()
        {
            if (this.Media.Source == MediaSource.Self)
            {
                var fileName = Config.MediaDirPath + this.Media.FilePath.Replace(MediaDirPath, "");

                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
            }
        }

        public void SetMediaUri()
        {
            if (this.Media.Source == MediaSource.Self)
            {
                this.Media.Uri = Config.ServerPath + "/api/v1/download" + this.Media.FilePath + (this.AuthData?.IsAuthed == true ? "/token?token=" + Uri.EscapeDataString(this.AuthData.AuthToken.AccessToken) : "");
            }
            else
            {
                this.Media.Uri = this.Media.FilePath;
            }
        }
    }
}
