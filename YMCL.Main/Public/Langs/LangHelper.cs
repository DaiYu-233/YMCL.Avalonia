using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using YMCL.Main.Public.Classes;

namespace YMCL.Main.Public.Langs
{
    public class LangHelper : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private static readonly LangHelper _current = new LangHelper();
        public static LangHelper Current => _current;

        readonly MainLang resource = new MainLang();
        public MainLang Resources => resource;

        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = this.PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public void ChangedCulture(string name)
        {
            if (name == "" || name == null || name == "Unset")
            {
                MainLang.Culture = CultureInfo.GetCultureInfo("zh-CN");
            }
            else
            {
                MainLang.Culture = CultureInfo.GetCultureInfo(name);
            }
            this.RaisePropertyChanged("Resources");
        }

        public string GetText(string name, string culture = "")
        {
            var setting = JsonConvert.DeserializeObject<Setting>(File.ReadAllText(Const.SettingDataPath));
            CultureInfo cultureInfo;
            if (culture == "")
            {
                if (setting.Language == "zh-CN" || setting.Language == null)
                {
                    cultureInfo = CultureInfo.GetCultureInfo("");
                }
                else
                {
                    cultureInfo = CultureInfo.GetCultureInfo(setting.Language);
                }
            }
            else
            {
                cultureInfo = CultureInfo.GetCultureInfo(culture);
            }
            var res = MainLang.ResourceManager.GetObject(name, cultureInfo).ToString();
            if (res == null)
            {
                return "Null";
            }
            else
            {
                return res;
            }
        }
    }
}
