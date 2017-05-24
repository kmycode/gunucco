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

        public AuthenticationData AuthData { get; set; }

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

        private void CreateMediaDirectory()
        {
            if (!Directory.Exists("." + MediaDirPath))
            {
                Directory.CreateDirectory("." + MediaDirPath);
            }
        }

        public void SaveMediaAsFile()
        {
            if (this.Content.Type == ContentType.Image && this.Media.Source == MediaSource.Self)
            {
                this.Media.FilePath = MediaDirPath + $"{this.Media.Id:00000000}";
                this.CreateMediaDirectory();

                FileStream fs = new FileStream("." + this.Media.FilePath, FileMode.Create);
                BinaryWriter bw = new BinaryWriter(fs);
                bw.Write(this.Media.MediaDataRow);
                bw.Dispose();
                fs.Dispose();
            }
        }

        public void LoadMediaFromFile()
        {
            if (this.Media.Source == MediaSource.Self)
            {
                var fi = new FileInfo("." + this.Media.FilePath);
                if (!fi.Exists)
                {
                    throw new GunuccoException(new ApiMessage
                    {
                        StatusCode = 404,
                        Message = "Media file is not found.",
                    });
                }
                var fileSize = (int)fi.Length;

                FileStream fs = new FileStream("." + this.Media.FilePath, FileMode.Open, FileAccess.Read);
                BinaryReader br = new BinaryReader(fs);
                this.Media.MediaDataRow = br.ReadBytes(fileSize);
                br.Dispose();
                fs.Dispose();
            }
        }

        public void DeleteMediaFile()
        {
            if (File.Exists("." + this.Media.FilePath))
            {
                File.Delete("." + this.Media.FilePath);
            }
        }
    }
}
