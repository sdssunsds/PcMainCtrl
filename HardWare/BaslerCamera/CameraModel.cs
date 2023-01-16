using Basler.Pylon;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PcMainCtrl.HardWare.BaslerCamera
{
    public class CameraModel : ICameraInfo
    {
        private Dictionary<string, string> dict = new Dictionary<string, string>();

        public string this[string key] { get { return dict[key]; } }

        public bool ContainsKey(string key)
        {
            return dict.ContainsKey(key);
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public string GetValueOrDefault(string key, string defaultValue)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    public class CameraData : Camera
    {
        public string Name { get; set; }
        public string Path { get; set; }
    }
}
