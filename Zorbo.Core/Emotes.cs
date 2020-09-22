using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Zorbo.Core
{
    public static class Emotes
    {
        public static readonly Regex FullRegex;
        public static readonly Regex StripRegex;
        public static readonly Regex TopicRegex;

        public static readonly List<Emoticon> Images;

        public static string GetImageUri(string key)
        {
            var emoticon = Images.Find((s) => s.Key == key);
            if (emoticon == null) { return null; }

            return emoticon.Image;
        }

        public static string StripTopic(string text)
        {
            int lastMatch = 0;
            string text2 = string.Empty;

            if (string.IsNullOrEmpty(text))
                return string.Empty;

            Match match = StripRegex.Match(text);
            while (match.Success) {
                text2 += text[lastMatch..match.Index];
                lastMatch = match.Index + match.Length;
                switch (match.Value) {
                    case "\u0003":
                        if (text.Length >= lastMatch + 7 && text[lastMatch] == '#') {
                            lastMatch += 7;
                            break;
                        }

                        if (text.Length >= lastMatch + 2 && int.TryParse(text.Substring(lastMatch, 2), out _))
                            lastMatch += 2;
                        break;
                    case "\u0005":
                        if (text.Length >= lastMatch + 7 && text[lastMatch] == '#') {
                            lastMatch += 7;
                            break;
                        }
                        if (text.Length >= lastMatch + 2 && int.TryParse(text.Substring(lastMatch, 2), out _)) {
                            lastMatch += 2;
                        }
                        break;
                }   
                match = StripRegex.Match(text, lastMatch);
            }
            text2 += text[lastMatch..];
            return text2;
        }

		static Emotes() {
            Images = new List<Emoticon> {
                new Emoticon(":)", "images/emotes/smile.gif"),
                new Emoticon(":-)", "images/emotes/smile.gif"),
                new Emoticon(":D", "images/emotes/grin.gif"),
                new Emoticon(":-D", "images/emotes/grin.gif"),
                new Emoticon(";)", "images/emotes/wink.gif"),
                new Emoticon(";-)", "images/emotes/wink.gif"),
                new Emoticon(":O", "images/emotes/omg.gif"),
                new Emoticon(":-O", "images/emotes/omg.gif"),
                new Emoticon(":P", "images/emotes/tongue.gif"),
                new Emoticon(":-P", "images/emotes/tongue.gif"),
                new Emoticon("(H)", "images/emotes/shades.gif"),
                new Emoticon(":@", "images/emotes/angry.gif"),
                new Emoticon(":$", "images/emotes/blush.gif"),
                new Emoticon(":-$", "images/emotes/blush.gif"),
                new Emoticon(":S", "images/emotes/confused.gif"),
                new Emoticon(":-S", "images/emotes/confused.gif"),
                new Emoticon(":(", "images/emotes/sad.gif"),
                new Emoticon(":-(", "images/emotes/sad.gif"),
                new Emoticon(":'(", "images/emotes/cry.gif"),
                new Emoticon(":|", "images/emotes/what.gif"),
                new Emoticon(":-|", "images/emotes/what.gif"),
                new Emoticon(":#", "images/emotes/sealed.gif"),
                new Emoticon(":-#", "images/emotes/sealed.gif"),
                new Emoticon("8O|", "images/emotes/snarl.gif"),
                new Emoticon("8-|", "images/emotes/nerd.gif"),
                new Emoticon("^O)", "images/emotes/sarcastic.gif"),
                new Emoticon(":-*", "images/emotes/secret.gif"),
                new Emoticon(":^)", "images/emotes/idk.gif"),
                new Emoticon("*-)", "images/emotes/think.gif"),
                new Emoticon("+O(", "images/emotes/puke.gif"),
                new Emoticon("<O)", "images/emotes/party.gif"),
                new Emoticon("8-)", "images/emotes/eyeroll.gif"),
                new Emoticon("|-)", "images/emotes/tired.gif"),
                new Emoticon("(6)", "images/emotes/devil.gif"),
                new Emoticon("(A)", "images/emotes/angel.gif"),
                new Emoticon("(L)", "images/emotes/heart.gif"),
                new Emoticon("(U)", "images/emotes/broken.gif"),
                new Emoticon("(M)", "images/emotes/messenger.gif"),
                new Emoticon("(@)", "images/emotes/cat.gif"),
                new Emoticon("(&)", "images/emotes/dog.gif"),
                new Emoticon("(S)", "images/emotes/moon.gif"),
                new Emoticon("(*)", "images/emotes/star.gif"),
                new Emoticon("(~)", "images/emotes/film.gif"),
                new Emoticon("(E)", "images/emotes/envelope.gif"),
                new Emoticon("(8)", "images/emotes/note.gif"),
                new Emoticon("(F)", "images/emotes/rose.gif"),
                new Emoticon("(W)", "images/emotes/wilted.gif"),
                new Emoticon("(O)", "images/emotes/clock.gif"),
                new Emoticon("(K)", "images/emotes/kiss.gif"),
                new Emoticon("(G)", "images/emotes/present.gif"),
                new Emoticon("(^)", "images/emotes/cake.gif"),
                new Emoticon("(P)", "images/emotes/camera.gif"),
                new Emoticon("(I)", "images/emotes/lightbulb.gif"),
                new Emoticon("(C)", "images/emotes/coffee.gif"),
                new Emoticon("(T)", "images/emotes/phone.gif"),
                new Emoticon("({)", "images/emotes/guy_hug.gif"),
                new Emoticon("(})", "images/emotes/girl_hug.gif"),
                new Emoticon("(B)", "images/emotes/beer.gif"),
                new Emoticon("(D)", "images/emotes/martini.gif"),
                new Emoticon("(Z)", "images/emotes/guy.gif"),
                new Emoticon("(X)", "images/emotes/girl.gif"),
                new Emoticon("(Y)", "images/emotes/thumbs_up.gif"),
                new Emoticon("(N)", "images/emotes/thumbs_down.gif"),
                new Emoticon(":[", "images/emotes/bat.gif"),
                new Emoticon(":-[", "images/emotes/bat.gif"),
                new Emoticon("(MO)", "images/emotes/coins.gif"),
                new Emoticon("(BAH)", "images/emotes/sheep.gif"),
                new Emoticon("(ST)", "images/emotes/rain.gif"),
                new Emoticon("(LI)", "images/emotes/storm.gif"),
                new Emoticon("(SN)", "images/emotes/snail.gif"),
                new Emoticon("(PL)", "images/emotes/dishes.gif"),
                new Emoticon("(||)", "images/emotes/chopsticks.gif"),
                new Emoticon("(PI)", "images/emotes/pizza.gif"),
                new Emoticon("(SO)", "images/emotes/soccer.gif"),
                new Emoticon("(AU)", "images/emotes/car.gif"),
                new Emoticon("(AP)", "images/emotes/airplane.gif"),
                new Emoticon("(UM)", "images/emotes/umbrella.gif"),
                new Emoticon("(IP)", "images/emotes/paradise.gif"),
                new Emoticon("(CO)", "images/emotes/computer.gif"),
                new Emoticon("(MP)", "images/emotes/cellphone.gif"),
            };
            string emote_pat = BuildEmoteRegex();
            string strip_pat = "\u0003|\u0005|\u0006|\a|\t";
            string link_pat = "arlnk://|www\\.|https?://|ftps?://|wss?://";
            StripRegex = new Regex(strip_pat, RegexOptions.Singleline | RegexOptions.IgnoreCase);
            TopicRegex = new Regex(strip_pat + "|" + emote_pat, RegexOptions.Singleline | RegexOptions.IgnoreCase);
            FullRegex = new Regex(strip_pat + "|" + link_pat + "|" + emote_pat, RegexOptions.Singleline | RegexOptions.IgnoreCase);
        }

        static string BuildEmoteRegex()
        {
            var sb = new StringBuilder();
            foreach (Emoticon emote in Images)
                sb.AppendFormat("{0}|", Regex.Escape(emote.Key));
            sb.Remove(sb.Length - 1, 1);
            return sb.ToString();
        }
    }

    public class Emoticon { 
    
        public string Key { get; set; }

        public string Image { get; set; }

        public Emoticon() { }

        public Emoticon(string key, string file) {
            Key = key;
            Image = file;
        }
    }
}
