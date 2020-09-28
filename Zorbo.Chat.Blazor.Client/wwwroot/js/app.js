(function ($) {

	$(window).on('load', async function () {

		function onChatResized(entries) {
			for (let entry of entries)
				ZorboApp.scrollToBottom(entry.target);
		}

		function escapeRegExp(string) {
			return string.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
		}

		const chatScreenObserver = new ResizeObserver(onChatResized);

		const stripPat = '\u0003|\u0005|\u0006|\u0007|\u0009';
		const linksPat = '\\\\\\\\|arlnk://|www\\.|https?://|ftps?://|wss?://';

		window.ZorboApp = {
			stripRegex: null,
			topicRegex: null,
			fullRegex: null,
			channelList: null,
			init: function () {
				var emo = this.emoteRegExp();
				this.stripRegex = new RegExp(stripPat, 'gi');
				this.topicRegex = new RegExp(stripPat + '|' + emo, 'gi');
				this.fullRegex = new RegExp(stripPat + '|' + linksPat + '|' + emo, 'gi');
			},
			setChannelList: function (list) {
				this.channelList = list;
				$('.attach-btn').on('click', function () {
					$(this).find('input').trigger('click');
				});
            },
			//elem shoud be <table> passed from .net
			addChannels: async function (table, json) {
			    var channels = JSON.parse(json);
				var obody = table.getElementsByTagName("tbody")[0];
				var body = document.createElement("tbody");
				for (let i = 0; i < channels.length; i++)
					await this.addChannel(body, channels[i]);
				table.removeChild(obody);
				table.appendChild(body);
			},
			addChannel: async function (elem, channel) {
				var row = document.createElement("tr");
				row.className = "channel";
				row.__channel__ = channel;
				row.onclick = function (e) { e.preventDefault(); }
				row.ondblclick = async function () { await ZorboApp.onChannelClick(row); };
				row.ontouchend = async function () { await ZorboApp.onChannelClick(row); };
				var name = document.createElement("td");
				name.className = "col name";
				name.title = channel.Name;
				name.innerText = channel.Name;
				row.appendChild(name);
				var topic = document.createElement("td");
				topic.className = "col topic";
				topic.title = channel.BareTopic;
				row.appendChild(await this.formatTopic(topic, channel.Topic));
				elem.appendChild(row);
            },
			onChannelClick: async function (elem) {
				await this.channelList.invokeMethodAsync('OpenChannel', JSON.stringify(elem.__channel__));
			},
			initResize: function (elem) {
				if (elem)
					chatScreenObserver.observe(elem);
			},
			scrollToBottom: function (elem) {
				if (this.selectionLength(elem) == 0)
					elem.scrollTop = (elem.scrollHeight + 50);
            },
			readCookie: function (name) {
				var match = document.cookie.match(RegExp('(?:^|;\\s*)' + escapeRegExp(name) + '=([^;]*)'));
				return match ? match[1] : "";
			},
			writeCookie: function (name, value, days) {
				var expires = "";
				if (days) {
					var date = new Date();
					date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
					expires = "; expires=" + date.toGMTString();
				}
				document.cookie = name + "=" + value + expires + "; SameSite=Strict; path=/";
			},
			attachFile: function () {
				document.getElementById('imgUpload').click();
            },
			showNameDialog: function () {
				var dialog = $('#username-dialog');
				if (!dialog.__zon__) {
					dialog.__zon__ = function (e) { dialog.addClass('hidden'); };
					dialog.__zoff__ = function (e) { dialog.removeClass('hidden'); };
					dialog.on('shown.bs.modal', dialog.__zoff__);
					dialog.on('hidden.bs.modal', dialog.__zon__);
				}
				dialog.modal('show');
			},
			hideNameDialog: function () {
				$('#username-dialog').modal('hide');
			},
			showHashlinkDialog: function () {
				var dialog = $('#hashlink-dialog');
				if (!dialog.__zon__) {
					dialog.__zon__ = function (e) { dialog.addClass('hidden'); };
					dialog.__zoff__ = function (e) { dialog.removeClass('hidden'); };
					dialog.on('shown.bs.modal', dialog.__zoff__);
					dialog.on('hidden.bs.modal', dialog.__zon__);
				}
				dialog.modal('show');
			},
			hideHashlinkDialog: function () {
				$('#hashlink-dialog').modal('hide');
			},
			selectionLength: function (elem) {
				if (!elem) return 0;
				var select = window.getSelection();
				var length = select.toString().length;
				if (length > 0) {
					var range = select.getRangeAt(0);
					if (range.startContainer == elem || range.endContainer == elem)
						return length;
					var ancestor = range.commonAncestorContainer;
					if (ancestor == elem) return length;
					while (ancestor.parentNode) {
						if (ancestor.parentNode == elem) return length;
						ancestor = ancestor.parentNode;
					}
                }
				return 0;
			},
			addTime: async function (helper, elem, message, startColor) {
				await this.addChatElement(elem, async function () {
					var ielem = document.createElement('span');
					ielem.className = 'text-cont time';
					return await ZorboApp.formatText(helper, ielem, message, startColor ? startColor : "gray");
				});
            },
			addPublic: async function (helper, elem, name, message, nc, tc) {
				await this.addChatElement(elem, async function () {
					var ielem = document.createElement('span');
					ielem.className = 'text-cont';
					var nelem = document.createElement('span');
					nelem.className = 'chatname';
					nelem.style.color = nc ? nc : 'black';
					nelem.innerText = name + '> ';
					ielem.appendChild(nelem);
					return await ZorboApp.formatText(helper, ielem, message, tc ? tc : "blue");
				});
			},
			addEmote: async function (helper, elem, name, message, startColor) {
				await this.addChatElement(elem, async function () {
					var ielem = document.createElement('span');
					ielem.className = 'text-cont';
					var nelem = document.createElement('span');
					nelem.className = 'chatname';
					nelem.style.color = startColor ? startColor : 'purple';
					nelem.innerText = '* ' + name + ' ';
					ielem.appendChild(nelem);
					return await ZorboApp.formatText(helper, ielem, message, nelem.style.color);
				});
			},
			addScribble: async function (elem, base64) {
				await this.addChatElement(elem, async function () {
					var ielem = document.createElement('span');
					ielem.className = 'scribble';
					var nelem = document.createElement('img');
					nelem.src = 'data:image/jpeg;charset=utf-8;base64, ' + base64;
					nelem.alt = 'Room Scribble';
					ielem.appendChild(nelem);
					return ielem;
				}, 100);
            },
			addAnnounce: async function (helper, elem, message, startColor) {
				await this.addChatElement(elem, async function () {
					var ielem = document.createElement('span');
					ielem.className = 'text-cont';
					return await ZorboApp.formatText(helper, ielem, message, startColor ? startColor : "navy");
				});
			},
			addChatElement: async function (elem, selector, delay) {
				if (elem) {
					elem.appendChild(await selector());
					if (!delay)
						this.scrollToBottom(elem);
					else
						setTimeout(function () { ZorboApp.scrollToBottom(elem) }, delay);
				}
				else {
					console.log('Element was null.');
                }
			},
			onHashlinkClick: async function (e, elem) {
				if (elem.__helper__ && elem.__channel__) {
					e.stopPropagation();
					e.preventDefault();
					await elem.__helper__.invokeMethodAsync('OpenChannel', JSON.stringify(elem.__channel__));
				}
            },
			clearScreen: function (elem) {
				if (elem) elem.innerHTML = "";
            },
			formatTopic: async function (ielem, message) {
				var cont = document.createElement('span');
				cont.className = "d-flex-row topic-cont";
				cont.appendChild(await this.formatText(null, ielem, message, "black", "transparent", "topic ", this.topicRegex));
				return cont;
            },
			formatText: async function (helper, ielem, message, startfg, startbg, baseClass, regexp) {
				let text = '';
				let b = false,
					i = false,
					u = false,
					fg = startfg ? startfg : 'black',
					bg = startbg ? startbg : 'transparent',
					last = 0,
					match = null,
					celem = null;
				if (message.length == 0) {
					celem = document.createElement('span');
					celem.className = this.getClass(b, i, u, baseClass);
					celem.style.color = fg;
					celem.style.backgroundColor = bg;
					celem.innerText = " ";
					ielem.appendChild(celem);
					return ielem;
                }
				if (!regexp) regexp = this.fullRegex;
				regexp.lastIndex = 0;
				while (match = regexp.exec(message)) {
					let mstr = match.toString();
					text = message.substr(last, match.index - last);
					if (text && text.length > 0) {
						celem = document.createElement('span');
						celem.className = this.getClass(b, i, u, baseClass);
						celem.style.color = fg;
						celem.style.backgroundColor = bg;
						celem.innerText = text;
						ielem.appendChild(celem);
                    }
					last = match.index + mstr.length;
					switch (mstr) {
						case '\\\\':
							break;
						case 'www.':
						case 'ftp://':
						case 'ftps://':
						case 'http://':
						case 'https://':
							var index = message.indexOf('\x20');
							var aelem = document.createElement("a");
							var atext = index > 0 ? message.substr(match.index, index - match.index) : message.substr(match.index);
							aelem.className = this.getClass(b, i, u, baseClass);
							aelem.href = atext;
							aelem.target = "_blank";
							aelem.innerText = atext;
							ielem.appendChild(aelem);
							last += atext.length;
							break;
						case 'arlnk://':
							var index = message.indexOf('\x20');
							var aelem = document.createElement("a");
							aelem.className = this.getClass(b, i, u, baseClass);
							var atext = index > 0 ? message.substr(last, index - last) : message.substr(last);
							if (helper) {
								var s = await helper.invokeMethodAsync("DecodeHashlink", atext);

								var channel = JSON.parse(s);
								if (channel) {
									aelem.__helper__ = helper;
									aelem.__channel__ = channel;

									aelem.href = "#";
									aelem.addEventListener("click", function (e) {
										ZorboApp.onHashlinkClick(e, this);
									}, false);

									var xelem = document.createElement('img');
									xelem.src = "images/chat.png";
									xelem.alt = "Channel hashlink";

									var yelem = document.createElement('span');
									yelem.innerText = channel.Name;

									aelem.appendChild(xelem);
									aelem.appendChild(yelem);
									ielem.appendChild(aelem);
								}
							} else {
								aelem.href = "#";
								aelem.innerText = "arlnk://" + atext;
								ielem.appendChild(aelem);
							}
							last += atext.length;
							break;
						case '\u0003':
							if (message.length >= last + 7 && this.charAt(message, last) == '#') {
								fg = message.substr(last, 7);
								last += 7;
							}
							else if (message.length >= last + 2) {
								fg = this.toHtmlColor(parseInt(message.substr(last, 2)));
								last += 2;
                            }
							break;
						case '\u0005':
							if (message.length >= last + 7 && this.charAt(message, last) == '#') {
								bg = message.substr(last, 7);
								last += 7;
							}
							else if (message.length >= last + 2) {
								bg = this.toHtmlColor(parseInt(message.substr(last, 2)));
								last += 2;
							}
							break;
						case '\u0006': b = !b; break;
						case '\u0007': u = !u; break;
						case '\u0009': i = !i; break;
						default://emotes
							var emstr = this.getEmoticon(mstr);
							if (emstr) {
								celem = document.createElement('span');
								celem.className = this.getClass(b, i, u, baseClass) + ' emote';
								celem.style.color = fg;
								celem.style.backgroundColor = bg;
								let img = document.createElement('img');
								img.src = emstr;
								img.alt = mstr;
								celem.appendChild(img);
								ielem.appendChild(celem);
                            }
							break;
					}
					regexp.lastIndex = last;
				}
				text = message.substr(last, message.length - last);
				if (text && text.length > 0) {
					celem = document.createElement('span');
					celem.className = this.getClass(b, i, u, baseClass);
					celem.style.color = fg;
					celem.style.backgroundColor = bg;
					celem.innerText = text;
					ielem.appendChild(celem);
				}
				return ielem;
			},
			stripText: function (message, regexp) {
				let text = '';
				let last = 0,
					match = null;
				if (!regexp)
					regexp = this.fullRegex;
				regexp.lastIndex = 0;
				while (match = regexp.exec(message)) {
					let mstr = match.toString();
					text += message.substr(last, match.index - last);
					last = match.index + mstr.length;
					switch (mstr) {
						case '\u0003':
						case '\u0005':
							if (message.length >= last + 7 && this.charAt(message, last) == '#') {
								last += 7;
							}
							else if (message.length >= last + 2) {
								last += 2;
							}
							break;
					}
					regexp.lastIndex = last;
				}
				return text + message.substr(last, message.length - last);
			},
			setValue: function (elem, value) {
				if (elem) elem.value = value;
            },
			getClass: function (b, i, u, base) {
				var str = base ? base : 'chattext ';
				if (b) str += 'bold ';
				if (i) str += 'italic ';
				if (u) str += 'uline ';
				return str.trim();
            },
			getEmoticon: function (key) {
				key = key.toUpperCase();
				for (let i = 0; i < this.emotes.length; i++) {
					if (this.emotes[i][0] == key)
						return this.emotes[i][1];
				}
				return null;
            },
			toHtmlColor: function (num) {
				switch (num) {
					case 0: return "#FFFFFF";
					case 1: return "#000000";
					case 2: return "#000080";
					case 3: return "#008000";
					case 4: return "#FF0000";
					case 5: return "#800000";
					case 6: return "#800080";
					case 7: return "#FFA500";
					case 8: return "#FFFF00";
					case 9: return "#00FF00";
					case 10: return "#008080";
					case 11: return "#00FFFF";
					case 12: return "#0000FF";
					case 13: return "#FF00FF";
					case 14: return "#808080";
					case 15: return "#C0C0C0";
					default: return "";
                }
			},
			charAt: function (str, index) {
				return str.substr(index, 1);
			},
			emoteRegExp: function() {
				let str = '';
				for (let i = 0; i < this.emotes.length; i++) {
					str += escapeRegExp(this.emotes[i][0]);
					if (i < (this.emotes.length - 1))
						str += '|';
				}
				return str;
			},
			emotes: [
				[ ":)", "images/emotes/smile.gif" ],
				[ ":-)", "images/emotes/smile.gif" ],
				[ ":D", "images/emotes/grin.gif" ],
				[ ":-D", "images/emotes/grin.gif" ],
				[ ";)", "images/emotes/wink.gif" ],
				[ ";-)", "images/emotes/wink.gif" ],
				[ ":O", "images/emotes/omg.gif" ],
				[ ":-O", "images/emotes/omg.gif" ],
				[ ":P", "images/emotes/tongue.gif" ],
				[ ":-P", "images/emotes/tongue.gif" ],
				[ "(H)", "images/emotes/shades.gif" ],
				[ ":@", "images/emotes/angry.gif" ],
				[ ":$", "images/emotes/blush.gif" ],
				[ ":-$", "images/emotes/blush.gif" ],
				[ ":S", "images/emotes/confused.gif" ],
				[ ":-S", "images/emotes/confused.gif" ],
				[ ":(", "images/emotes/sad.gif" ],
				[ ":-(", "images/emotes/sad.gif" ],
				[ ":'(", "images/emotes/cry.gif" ],
				[ ":|", "images/emotes/what.gif" ],
				[ ":-|", "images/emotes/what.gif" ],
				[ ":#", "images/emotes/sealed.gif" ],
				[ ":-#", "images/emotes/sealed.gif" ],
				[ "8O|", "images/emotes/snarl.gif" ],
				[ "8-|", "images/emotes/nerd.gif" ],
				[ "^O)", "images/emotes/sarcastic.gif" ],
				[ ":-*", "images/emotes/secret.gif" ],
				[ ":^)", "images/emotes/idk.gif" ],
				[ "*-)", "images/emotes/think.gif" ],
				[ "+O(", "images/emotes/puke.gif" ],
				[ "<O)", "images/emotes/party.gif" ],
				[ "8-)", "images/emotes/eyeroll.gif" ],
				[ "|-)", "images/emotes/tired.gif" ],
				[ "(6)", "images/emotes/devil.gif" ],
				[ "(A)", "images/emotes/angel.gif" ],
				[ "(L)", "images/emotes/heart.gif" ],
				[ "(U)", "images/emotes/broken.gif" ],
				[ "(M)", "images/emotes/messenger.gif" ],
				[ "(@)", "images/emotes/cat.gif" ],
				[ "(&)", "images/emotes/dog.gif" ],
				[ "(S)", "images/emotes/moon.gif" ],
				[ "(*)", "images/emotes/star.gif" ],
				[ "(~)", "images/emotes/film.gif" ],
				[ "(E)", "images/emotes/envelope.gif" ],
				[ "(8)", "images/emotes/note.gif" ],
				[ "(F)", "images/emotes/rose.gif" ],
				[ "(W)", "images/emotes/wilted.gif" ],
				[ "(O)", "images/emotes/clock.gif" ],
				[ "(K)", "images/emotes/kiss.gif" ],
				[ "(G)", "images/emotes/present.gif" ],
				[ "(^)", "images/emotes/cake.gif" ],
				[ "(P)", "images/emotes/camera.gif" ],
				[ "(I)", "images/emotes/lightbulb.gif" ],
				[ "(C)", "images/emotes/coffee.gif" ],
				[ "(T)", "images/emotes/phone.gif" ],
				[ "({)", "images/emotes/guy_hug.gif" ],
				[ "(})", "images/emotes/girl_hug.gif" ],
				[ "(B)", "images/emotes/beer.gif" ],
				[ "(D)", "images/emotes/martini.gif" ],
				[ "(Z)", "images/emotes/guy.gif" ],
				[ "(X)", "images/emotes/girl.gif" ],
				[ "(Y)", "images/emotes/thumbs_up.gif" ],
				[ "(N)", "images/emotes/thumbs_down.gif" ],
				[ ":[", "images/emotes/bat.gif" ],
				[ ":-[", "images/emotes/bat.gif" ],
				[ "(MO)", "images/emotes/coins.gif" ],
				[ "(BAH)", "images/emotes/sheep.gif" ],
				[ "(ST)", "images/emotes/rain.gif" ],
				[ "(LI)", "images/emotes/storm.gif" ],
				[ "(SN)", "images/emotes/snail.gif" ],
				[ "(PL)", "images/emotes/dishes.gif" ],
				[ "(||)", "images/emotes/chopsticks.gif" ],
				[ "(PI)", "images/emotes/pizza.gif" ],
				[ "(SO)", "images/emotes/soccer.gif" ],
				[ "(AU)", "images/emotes/car.gif" ],
				[ "(AP)", "images/emotes/airplane.gif" ],
				[ "(UM)", "images/emotes/umbrella.gif" ],
				[ "(IP)", "images/emotes/paradise.gif" ],
				[ "(CO)", "images/emotes/computer.gif" ],
				[ "(MP)", "images/emotes/cellphone.gif" ],
			]
		};

		window.ZorboApp.init();

	});

})(window.jQuery);