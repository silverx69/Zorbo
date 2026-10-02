using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zorbo.Ares.Packets.Chatroom;
using Zorbo.Core;

namespace Zorbo.Ares.Server
{
    public static class Captcha
    {
        public static int Create(AresClient client)
        {
            int total = 40;
            int emote = Utils.Random.Next(0, emoticons.Length);
            int count = Utils.Random.Next(5, 12);

            string name = names[emote];
            string noun = nouns[Utils.Random.Next(0, nouns.Length)];
            string end = ends[Utils.Random.Next(0, ends.Length)];

            string question = String.Format("\x000314How many \x0006{0}\x0006 {1} {2}?", name, noun, end);

            int[] random = new int[count];

            for (int i = 0; i < count; i++) {

                int index = Utils.Random.Next(0, total);

                while (random.Contains(index))
                    index = Utils.Random.Next(0, total);

                random[i] = index;
            }

            client.Socket.SendAsync(new Announce("\x000314Welcome to the room " + client.Name));
            client.Socket.SendAsync(new Announce("\x000314Please answer the following question:"));

            client.Socket.SendAsync(new Announce(""));
            StringBuilder sb = new StringBuilder();

            int current = 0;
            for (int i = 0; i < total; i++) {

                if (random.Contains(i))
                    sb.Append(emoticons[emote]);
                else {
                    int decoy = Utils.Random.Next(0, emoticons.Length);

                    while (decoy == emote)
                        decoy = Utils.Random.Next(0, emoticons.Length);

                    sb.Append(emoticons[decoy]);
                }

                sb.Append(" ");

                if (++current >= 8) {
                    client.Socket.SendAsync(new Announce(sb.ToString()));

                    sb.Clear();
                    current = 0;
                }
            }

            if (current > 0)
                client.Socket.SendAsync(new Announce(sb.ToString()));

            client.Socket.SendAsync(new Announce(""));
            client.Socket.SendAsync(new Announce(question));

            return count;
        }

        static readonly string[] emoticons = new string[]
        {
            ":-)", ":-D", ";-)", ":-O", ":-P", "(H)", ":@", ":$", ":-S", ":-(",
            ":'(", ":-|", "(6)", "(A)", "(L)", "(U)", "(M)", "(@)", "(&)", "(S)",
            "(*)", "(~)", "(E)", "(8)", "(F)", "(W)", "(O)", "(K)", "(G)", "(^)",
            "(P)", "(I)", "(C)", "(T)", "({)", "(})", "(B)", "(D)", "(Z)", "(X)",
            "(Y)", "(N)", ":-[", "(1)", "(2)", "(3)", "(4)"
        };

        static readonly string[] names = new string[] { 
            "happy", "toothy grin", "wink", "surprised",
            "tongue", "cool guy", "angry", "embarrassed",
            "confused", "sad", "crying", "blank stare", "devil",
            "angel", "heart", "broken heart", "messenger",
            "cat", "dog", "moon", "star",
            "film", "envelope", "music note", "flower",
            "wilted flower", "clock", "kiss", "gift",
            "cake", "camera", "lightbulb", "coffee", 
            "telephone", "boy hug",  "girl hug", "beer mug", 
            "cocktail glass", "boy", "girl", "thumbs up",
            "thumbs down", "bat", "asl", "handcuff",
            "sun", "rainbow"
        };

        static readonly string[] nouns = new string[] {
            "smileys", "images", "pictures",  "emotes", "emoticons"
        };

        static readonly string[] ends = new string[] {
            "can you see",
            "do you see",
            "can you count",
            "are there",
            "are visible",
            "are on the screen",
        };
    }
}
