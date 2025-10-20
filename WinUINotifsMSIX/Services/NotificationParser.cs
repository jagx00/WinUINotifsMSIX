using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.UI.Notifications;
using WinUINotifsMSIX.ViewModels;
using static Azure.Core.HttpHeader;

namespace WinUINotifsMSIX.Services
{
    public class NotificationParser
    {
        public int VisibilityDefault = 6;
        public int NotifVisibilityThreshold = 6;
        public string[] ExcludeStartValues = new[] { "caption ", "ffffff", "mood." };

        // 20210421
        // Notifications Visualizer
        // 20210520
        // Decided that toasts generated from this App should be included for test reasons
        // I can just delete them from the list easily enough
        public string[] IncludeApps = new[] { "outlook", "google chrome", "microsoft edge", "phone link", "notifications visualizer", "twitter", "uwpexamplecs" };
        //public string[] ExcludeApps = new[] { "autoplay","calendar","gigabyte force","macrium reflect disk imaging and backup","macrium reflect","mail","microsoft store",
        //                                      "onedrive","outlook","phone link","suggested","snip & sketch","ssms 18","unknown",
        //                                      "windows explorer","windows security","skype","tips" };
        public string[] ExcludeApps = new[] { "autoplay","calendar","gigabyte force","macrium reflect disk imaging and backup","macrium reflect","mail","microsoft store",
                                              "onedrive","phone link","suggested","snip & sketch","ssms 18","unknown",
                                              "windows explorer","windows security","skype","tips" };
        public string[] IncludeTitle = new[] { "jagnotiftest", "john goudy","afl","afl just tweeted", "adelaide crows","brisbane lions","carlton fc","collingwood fc", "essendon fc", "footy on nine", "fox footy", "geelong",
                                               "herald sun","north melbourne","andrew sent",
                                               "twitter" ,"western bulldogs","sen 1116","tim gardner" };
        //"andrew sent",
        public string[] ExcludeTitle = new[] { "3aw","9news","aaron dodd - not a bot, or a trot","abc news","act policing",
                                               "adam knott","adella beaini","afl fantasy","afl trade whisperer","alan kohler",
                                               "andrew clennell","andy maher","annastacia palaszczuk","anthony klan","antony green","arad nik",
                                               "#bacheloretteau","barrie cassidy"," banks","ben keays","betfair au","box hill hawks",
                                               "brent hodgson"," briggs","brinsty harrison","brittany higgins","brodie grundy",
                                               "casey briggs","cate faehrmann","catherine andrews","carmen",
                                               "cave dweller","chris anstey","cj miller", "clare armstrong","corey mckernan","covidpete",
                                               "dan andrews","dane swan","daniel cherny","daniel gorringe","danielle grindlay","david culbert","david marler","david milner","david sharaz",
                                               "dr eric levi","dr queen victoria","dr rachel heath",
                                               "eden gillespie", "emma","emma alberici",
                                               "finnigans","francis leach","friendlyjordies",
                                               "halil mustafa","hanson-young",
                                               "georgie dent","georgie parker","graham cornes","guy sebastian",
                                               "henrietta cook",
                                               "ian graves","isabelle al","isobel roe",
                                               "jack grealish","jack silvagni","jane caro","janine perrett","jill stark",
                                               "jo dyer","jonathan greenwood","jon ralph","josh","j riewoldt","julian burnside","julie-ann finney","justine elliot",
                                               "karen middleton","kate","katharine murphy","katherine","kathiemelocco","katy gallagher","kevin rudd",
                                               "lara!","laura tingle","l e a h","lin jong","lisa wilkinson","louise milligan","lucy morris","luke hilakari","luke mcgregor","lyn",
                                               "mark allen","mark mcgowan","mark stevens","mark stewart","mark wahlberg","martin pakula","mason cox",
                                               "matilda boseley","matthew lappin","matthew richardson","max gawn",
                                               "michael hurley","michael rowland","mike carlton","mitch robinson",
                                               " murphy",
                                               "nat edwards","nathan buckley","neil mitchell","nick mcintosh","norman swan",
                                               "oliver yates",
                                               "patricia karvelas","patrick dangerfield","paul barry","paul james","paul meek","perth wildcats","peter doherty",
                                               "peter fitzsimons","peter fox","phillip riley","possum","prguy"," proposed ",
                                               "queensland",
                                               "rafael epstein","rashida yosufzai","ray martin","richard willingham","rick morton",
                                               "rohan connolly"," rowland","ryan daniels","ryan fitzgerald",
                                               "samantha maiden","sam edmund","sam mcclure","sandra"," sandro ","scott morrison","scott pendlebury","sharnelle vella",
                                               "shaun micallef","simon banks","simon love","solo monk","stephen parnis","steven may","supercoach afl",
                                               "talkingblue","taylor adams","thank you!","the australian","the chaser","the guardian","tim michell",
                                               "tom koutsantonis","tom morris","tommy bugg","tom swann","tony windsor",
                                               "vaccine tracker","vicgovdh","victoria police","virginia trioli","whistleblower","will schofield",
                                               "xavier ellis","xenophon davis","yve","zoe daniel"

                                               };

        // 20210729
        // The crown emoticon probably shldnt be in this excludetitle list as titles already excluded if they have an emoticon, so moved it there

        // 20210422 - makes includebodyvalues not apply if an override exists 
        // so for example: any text with 'changes' normally gets accepted as an include, but not if the text is 'coaching changes'
        // 20210616 - maybe the overrides are most of the excludes? eg mid-season changes tbi
        // 20230516 - also see CheckForLateChange which has some hard coded overrides
        public string[] IncludeBodyValuesOverrides = new[] { "#aflexchange","changes for their clash", " coaching changes","do you agree"," draft "," filmed ",
                                                             "get the four points"," handy ","highlights", " host of changes","ht:","lead changes","management","mid-season","mindset",
                                                             "olympics","parliament"," prediction "," preview ","qt:"," rule changes",
                                                             "subbed out"," unveil"," us open ","what a win","younger squad"};


        public string[] IncludeBodyValues = new[] { "afl.com.au/news/teams","a huge out", "a late out", "a very late out",
                                                    "being withdrawn","changes","details have been finalised","due to illness",
                                                    "final teams","fixture change","forced into quarantine","game will commence",
                                                    "as selected",
                                                    "has been replaced", "has been withdraw","has replaced","have been postponed","have been replaced", "he replaces ",
                                                    " in place of "," is out of ","late change", " late outs ",
                                                    "late withdrawal",
                                                    "line ups", "line-ups", "None of these players or staff will participate in today’s match",
                                                    "not expected to play" ,"replaced in selected" ," replaces " ,"ruled out of " ,"team update" ," to withdraw" ,"venues have been locked",
                                                    "was withdrawn","will be without","will miss today","will miss tonight","withdrawn from the team"};

        // 20210413 - separated
        public string[] ExcludeEmojis = new[]
             { "🎁",
               "😳","😍","😘","😎","🤕","😊","😯","😷","🤯","😜","🤑","😬","🤐","🤗","🤫","😅","😁","☺️","😢","😶","🌝","🤔","🤤","🥺","😉",
               "🤕","😇","😏","😮","👶","😂","😩","😌","🤢","😄","😛","🥰"," 🤩","😤","🙅‍","😲","😫","🤨","🥲","🤣","😝","🤠","🙂","🥹","🥹",
               "🥹", "🧐","🥵","🍊","🥶","🤭","🤷‍","😆","😍",
                            "💛","💙","🧡","💕","♥️","❤️","💔","💜","🤍","🖤","♥",
                            "🔟","2️⃣","3️⃣","4️⃣","6️⃣","🔝","🔛","🥂","🦢",
            //"👉",
            "🌡","🆚","🦘","☔️","🎶","👆","⤴️","🌭","🎣","🏠","⬇️","🍀","🎨","📌",


                            "🤳","🙏","🧤","🙌","👍","✋","👋","🤞","👏","🤘","👊","🖐","👌","👈","👇","💪","🤝","🦶","🔥","👀","👂","✌️","🤲","🤟",
                            "🎈","🍬",
            //"✈️",
            "😱","🆗","⚠️","🐍","💸","🔴","🔵","🏏","🦎","🍯","✊","🍷","✔️","🎟","💎","🔁","⤵️","🗣","🏹","↪️",
                            "🙈","💥","🙏🏼","📷","🎯","✨","✅","🤵🏻","💯","🏆","🌟","🤹‍","🏳️‍🌈", "💣","🔙","🚨","✍️","🏀","🐰","⌚️","🧑‍🍳","⚽",
                            "👔","🔒","🏅","🏡","💫","🍑","🏟","🧲","🆙","😀","🍒","™","🧙‍♂️","🧙‍","🍌","👨‍🎨","❄️","🎬","🖋","✂️", "🏍","💨","🕺",
                            "🦞", "📉","🫶","🤌","💧","🤯","🎵","📣","🧈","💍",
                            "👒","🏎","🏉","▁","💌","📍","🎙","🚂","🚀","🧱","💰","📆","🎞","⚪","☀️","⭐️","🌞","☘️","🎾","🍿","🛫","🌀","👑","🧊","♛","📈",
                            "📞","📸","🚦","⚡","🎸","🚢","➡️","🦁","🐶","🦄","😻","🐯","😼","🐐","🦅","🐱","⚓","😈","🧔"};

        // in alpha order bar the first stuff - trail by defeat etc
        public string[] ExcludeBodyValues = new[]
        {

                            "!","?",
                            " ahead.","another loss"," are home"," are in control"," are in front"," are in the eight","all-square",
                            " back in front"," back in-front"," back in the eight"," beat up on"," belting"," belt the "," big loss"," boilover "," bragging rights"," charged home", "comfortably accounting for","cruise to a ",
                            " do a number on "," defeat",
                            " feisty challenge", " fought off a",
                            " game on the line"," get it done"," hang on "," have overcome "," held off "," hold on "," hold on.", " in. front"," in front", " lead"," losing to"," mighty task ","four points",
                            "one point game","one point in it","outscored"," thrashed " ," trail by " ," trailing " ,"prove too strong","point game","point lead","point loss to ","point thrashing","points in it","pulls away from",
                            "scores are level","scores level","seals it","seals the deal","seals the game","slaughtered","streak ended",
                            " toppling "," two kick game"," unlikely draw " ," win " ," win, " , " wins " , " with a nice buffer" ,
                            "qt:" , " at ht" ,"ht:" ,"ht at the" ,"half time" ," the main break" ,"3qt" , "ft:","victorious"," victory "," were too good",
                            "goal","behind",
                            "3-2-1","100 games","200cm","200 games","200 up","250 x 2","300 games","50-metre penalty",
                            "@aflfantasy","@coles", "@rihanna",
                            "#aflw",
                            "aami"," aboriginal ","absolute jet","absolute track"," absolutely"," abused ",
                                " academy ","access all area","accomplished","according to ","accuracy"," achieved "," action ","activated their medical"," actually",
                                " adrenaline "," advantage ","adversarial", " advice "," advice.",
                                "a fan of ","aflcomau learned from","afl360","aflexchange","aflfantasy"," aflpa "," aflpa "," aflw ","aflwomen","aflw squad",
                                " aggressive "," agreed "," aiming "," airing ",
                                " alive ","all class"," alleged ","all-rounder",
                                "all square","almost ready"," altercation "," always ",
                                " amazing","ambassador"," amiss ",
                                " analyses ", " analysis "," angle "," angles "," angry ","animal"," annual ","another angle","anticipated","anticipation"," anywhere",
                                "apologies ","apologised","apology"," apparent "," appreciation"," approval",
                                " are back "," argues "," armbands ","armchair"," army "," around the body"," around the corner"," arrangements ",
                                //" arrive "," arrived ",
                                " arrow "," artist ",
                                " aspirations"," asset "," assistance"," assisted",
                                "athletics","atonement"," at stake"," attack "," attendance "," attorney"," attracting","ausgp","auskick","automatic",
                                " available ",
                                " awarded ", "awareness"," awesome",
                            "baaaa","back down","background","back in front"," backstroke","back-to-back",
                                "ball game","ball movement","ball over","banana","bang.",
                                //"banned",
                                " bargain ","baseball"," batting ","battle it ","battle of ","battling",
                                "beautiful","beauty"," believe "," believes "," belted ","bemused","bend it ","bends it ","bends one "," bend this ",
                                " best ever ","best on ground","better and better","betting "," bevy ","beware ","beware,",
                                "big big sound","big, big sound","biggest fan","biggest snag","big freeze","bigfreeze","big grabs","big kick","big men","big number","big problem","big race ","big start","big test",
                                //"bit.ly/", // often has info as well as link
                                "birthday","bit of space",
                                "blacklives"," bleak "," blessed "," blink ","blockbust"," blocks "," bloke "," bloke."," blood "," bloody ","bluntly",
                                "bobs up ", "bolstered "," bomb ","bookmark","boom "," boosted "," boot "," boots "," bottom of "," bounce back"," bounces",
                                "bowling","bows out.","boxing"," boys ",
                                " bracing ","brain fade","bravery"," bread ","breaking |" ,"breathe "," breathing "," breeze "," brewing "," brick ","brick wall","brilliance","brilliant",
                                "bring. it. on","bring on ","bring up a ton"," bristled",
                                " broke ","bronze medal"," brotherly ","brownlow","bruising","brutal.",
                                "buckle ","buddy"," budgie "," bumper "," bundle "," business","business "," busy "," buzz "," buzzer ",
                            " cake ","calculation","calendar"," calibre ","call him","calm before"," camera "," candy","cannot miss","captain's game",
                                "car crash","cardiac"," career "," career-best "," career-high "," careful "," caretaker "," cargo ","car park"," catch ","catch up with",
                                "celebrat"," cement ","centre square"," ceo ","ceo-elect "," ceremony "," certainty ",
                                "chairman","challenged"," champ "," champion "," champions "," championships ","chances around"," chaos "," chapter",
                                "charity","charges have been laid"," cheap ","checking in ","cheer squad","cherry "," chippy "," chips "," chunk ",
                                " circle "," circling ",
                                " clashes "," class "," classic"," classy finish","clearance ","clears throat","cleeeean"," clinical","clinical.",
                                " clock "," close loss "," closing in "," club of choice ",
                                //" coach",
                                " coalition"," coffin ",
                                "cold-blooded","coleman medal"," collapse "," collared ","collision"," colours ",
                                "combination","comeback","comes off","come to the bench"," comfortable "," comfy"," coming.","coming through",
                                "commentating", "community",
                                //"competition ",
                                "competitive ","complacency","comprising",
                                " concerned ",
                                //"concussion",
                                "condemned","confident","congestion","congratulations"," conquer "," consoles "," consecutive ",
                                " contemplating "," contest "," contested ",
                                //" continue ",
                                //" contract extension"," contracts exten",
                                "controversial"," controversy ","conundrum","converts",
                                " cooking ","countdown",
                                " could mean "," counselling "," counting "," country","courage"," costly ",
                                " cracker "," cracking ","crafty ","crazy","creates something","critiqued","cricket","cross-code"," crossroad"," crowd "," crucial"," crumb ","crumbing","crumbs",
                                " cult "," culture"," cultures "," curls "," curves ","cycling",
                            " dad "," dagger ","dance floor","dance move"," dances ","dancing ","danger ",
                                //"dangerous",
                                " dap ","daughter",
                                //"day is over",
                                "deadeye ","deadline day","debutants","decent grab","decision","decorated","deep breath"," defiant ",
                                " delhi "," delight","delisted","delisting"," delivered "," delivers "," delivery "," delves ",
                                " denied "," denies "," deny "," deserve "," desire "," desperate "," desperation","destiny ",
                                " diary"," did this "," difference","dilemma"," dime ","disadvantage"," disappointed"," disappointing ",
                                " discuss "," discussed "," discusses "," discussion ", "dislike","disposal", " dissect ","distraught",
                                "documentary","docu-series","does it again","doing his thing","dominance","dominant","dominated","dominating","domino",
                                " donate "," done it "," done with ","don't argue","don't say it"," doom ","do or die","do-over","dotted","downgraded ","down-graded ","downplayed",
                                " draft "," dream ",
                                "drought","dribble","drop whatever"," drought "," drug ",
                                " duality "," dusty."," dying "," dynamic ",
                            "early arrival","early in the game","earmuffs"," easier "," easy "," easy.","eddie's","eddie the eagle"," edging"," effort "," effortless ",
                                " electric "," elephant "," elite ","elite stuff",
                                //"emergency",
                                "emerging","emotional",
                                "end to end"," enemy","enjoy","enough said","entertain","entrants "," envy "," episode "," episodes ","errors","essential ",
                                "eternal flame"," eventful","excited ","excitement "," expectations"," explore ","extends"," eyes "," every time "," everytime ",
                                "exclusive","execution","executive",
                                //"expects",
                                "experimental"," explains"," explodes","extraordinary",
                            " face.","facebook"," face off ","fadeout","fair bit ","fair bump","fair effort","fair to say"," fairy "," faithful"," family",
                                "fan behaviour"," fan club "," fan favourite"," fans "," fans, ","fantastic",
                                "fascinating","fast feet","fast start","f a step","fat shaming",
                                "favourite player","favourite son",
                                " feature "," feedback "," feeling ",
                                " fiasco "," fiery"," fight","fightmnd"," figure ","fill in the blank",
                                "finals contention",
                                " finals mode"," final spot "," finals race","final term","final words",
                                "find out "," fined ","finishes","finishing touch","fired up",
                                //" first ",
                                "firstcrack","first term","fitting result",
                                //" fixture ",
                                " flagged "," flare "," flared "," flaring "," flee "," flex "," flies "," flight "," flowing ","fluctuating"," flush"," flyer ",
                                " focus "," focused "," focuses "," football"," footrace "," footwork ",
                                " forecasting"," forget ","for his second"," for life.","formalised his ","formalising"," former ","formula one","for six!","for the fans"," fortune ","forward line",
                                " foundation ", "four points","fourth major",
                                "franchise","frank assessment","free agency","freestyl","from range","front and centre","front bar","frontrunner","frustrating ",
                                "full approval"," fumes "," fun "," funding ","furniture",
                            " gallant "," galore ",
                                " game 100"," game 150"," game 200","game 250 "," game 250"," game 300","game ahead","game on.", "gameplan","game, set","game time","games total",
                                "gather and give", "gatorade",
                                "geez "," gender","genius"," gentle "," genuine ","get one back","get over the line ","get the choccies","get the chocolate","gets a much needed one","gets another","gets involved","gets one back",
                                "getaway","gets a late one","gets the first","gets the opener","get their man","get the job done","getting past","get your ",
                                " girls","give you a chance",
                                " glimmer "," glimpse "," glorious.","glue guys",
                                " go after "," gods ","goes bang","goes for home"," go hard.","gold medal"," golf "," golfer ","golf's"," gonna ",
                                " good.","good find","good grief","good hands","good luck","good month"," goodness","good night","goodnight","good number","good player",
                                "got a couple","got three","got eight","got one","got the moves",
                                " grab "," great ","grim."," ground "," grudge "," grudges ",
                                " guernsey"," guides ",
                                //" gun ",
                                "gymnast",
                            "had their say","haircut","half-time","hammering"," handball ","handing out","handy finish"," hanger"," hanging up ","happy ","hard hitting",
                                " has been activated"," has come off"," has come on as"," has died"," has five",
                                //" has entered"," has re-signed",
                                " has some moves",
                                " has his second"," has six."," has the first"," has three"," has two,"," has two.",
                                " have arrived"," have won ",
                                " headband","headed to the rooms"," headers "," headgear "," heading "," headlines "," heart ","heartbreaking"," heartened "," heat "," heated ","heave-ho",
                                "held on"," helmet",
                                //" helped off ",
                                " help us ",
                                "here comes ","here. we. go.","her future "," hero."," heroes ","he's home","hey siri,",
                                "high five","high hopes","highlight",
                                "his appreciation","his future","his height","his knee"," his old "," his preferred "," his side "," his sights"," history"," his views ",
                                " hobbled ","home & away","home sweet "," honour "," honouree"," hoodoo"," hopes "," hoping "," horrible "," hot "," houses ",
                                "how good ","how good.",
                                //" huge ",
                                "huge day out","huge mood","humanitarian","humiliating"," hung up "," hungry "," hunts "," hustles "," hyphen ",
                            "iced up"," ideal "," if you don't mind"," ignite ","ill-informed","illusion",
                                 "immediately"," immortal"," immunocompromised ",
                                 //" impact "," impact.",
                                 " impacting ","impossible","important win",
                                 "importance",
                                 //" important ",
                                 "impression","impressive",
                                 " in a row","inaugural",
                                 //" incident",
                                 "inconsistency","incredibl","indigenous"," indoor "," industry"," in every sense",
                                 " influence ",
                                 //" in-form ",
                                 "infringement",
                                 //"injury front","injury list","injury news","injury wrap",
                                 "insane "," inside 50"," insight "," inspiration","instagram",
                                 "integrity","intensity","intercepting","interested","interesting","international ","interview",
                                 "in the balance","into trouble","investigation","invitation",
                                 " iraq","irrelevant",
                                 //"is back",
                                 "is headed to","is heading to", "israeli ","is tracking",
                                 "it seems ","it's going","it's time","it turns out",
                            " jacket ", " jets "," journey "," jumper "," jury ",
                            "kayak","kayosports","keen.","keen to ","keeping an eye ","keep scrolling",
                                " kick ","kicked"," kicks ","kickstart","kidsmonth"," kinda ","kindness"," knock",
                            "ladder"," lament "," landing ","last night's"," latest on "," laugh "," launched "," launches "," launching "," lays down ",
                                " lead changes "," leaning "," leaps "," learning ","leave the club",
                                "left all alone","left-field","legacy","legend"," lessons ","lest we forget",
                                "let it rip","lets go","let's go"," letting them "," levels it "," level terms ","level the score"," levitating ",
                                " life member"," lights ","like father","like we said","listen to"," live now ","lloyd!",
                                " loads up "," lockdown","lol @","long-term","look away ","look back at "," looking ","loser goes","lost child"," lottery",
                                " love "," loved ","lovehiswork","lovely stuff","love this"," loving ","loyalty",
                                " lunch "," lure ",
                            "made it two wins","magic","magician","magnificent","majesty",
                                "make of this","makes a meal","makes them pay","making his mark"," malice ",
                                "management"," mantra "," margin ","margin call"," marks ","masterclass","masterstroke",
                                "match ball",
                                //"match centre",
                                "match preview","match report"," matchup","match winner"," mates ","maximum","mayday",
                                "mean feat"," meant that"," medal for "," medal tally"," media ","medical attention"," meetings "," mega "," members "," memes ","memorable","memories",
                                "mental health","merch update"," message "," mess with "," met with ",
                                " might be ","milestone","million dollars","mindset"," minutes left","minutes to go","minutes to play","minute to go","minute to play",
                                " missed "," mission "," mistake"," mlb ",
                                " modern "," moment "," moments "," momentum "," monster","monsterenergy"," month ",
                                " mood "," moon ","motogp"," motor "," motto"," moustache","movember"," movement "," movements ",
                                //" mro",
                                " mum ","must-read"," must stand up"," my absolute "," myth ",
                            "nail-bite","nailbiting"," nailed "," nailing "," nails "," narrated "," narrative "," national "," navigate ",
                                " nba ",
                                "nearly ","need to know"," nervous "," netball ","never gave ","never gets old","never in ","never missing ","new contract ","new deal ","new era ","new look ",
                                "next level","next month","next up ","next week",
                                "nice finish"," nice grab"," nicely","nickname","night footy",
                                "night's loss",
                                "no excuse","no further","no mercy"," nominated"," nominees"," nonno ","no problem","no stopping",
                                "not bad at all","notebook","not even surprised","now you see it","nrl ",
                            "offered contracts","off-field"," office "," officially on the move"," officially open","off-season","off the deck","of the year",
                                "olympian","olympic"," omg ",
                                "on a roll","on a string"," on demand","one hand ",
                                "one more ","one-on-one","one step closer","on the board","on the mark","on the road"," onus ",
                                "ooooo","opening ","open-minded","opens up","our mate","ourselves",
                                "ouch ","out-of-","out of the blocks","out of the game"," outlook ","outrageous","outstanding"," outta "," out the back",
                                "over the fence"," overturn "," oval ",
                            "pack mark","paddock"," panel "," panic "," parade ","paralympic"," parted ","paris games"," partner "," party "," party."," passionate"," pathway"," pay."," pay from ",
                                " peas "," pegs ","pen to paper","people"," percentage "," perfect "," perfection"," perfectly","perfect start","performance","performing",
                                "photography","photos by","phwoar",
                                "pick no.","picks out","pick swap","piling them on","pinch himself","pin drop"," pinpoints ","pivotal",
                                " plants ","play ahead","played ok"," players go.","players' night","player to watch","playing finals","playing like a ","playoff","play the victim",
                                " pleaded"," please.","please "," pleasure ","pledges","plenty of it","plenty to say",
                                "podcast"," poem "," point. game"," points for "," pokes "," pops ",
                                "positioning","positives","post-fight","post-game","post-high",
                                //" potential ",
                                " potentially "," pounce "," pounced "," pounces ","power ranking"," powers ",
                                "pppp",
                                " praise ", " praised ","praise da "," predict ","pre-game reading"," premier "," premiership ","pre-order",
                                " preparation "," preparations ","pre-season"," presented "," president", "press conference",
                                " presser "," pressure ","prestigious","pretty good"," preview ",
                                " privilege "," prix "," prix."," proceedings "," pronounce "," proposed "," prospects ",
                                //"protocol", // concuss inj
                                "psychologist",
                                " pulling ", " pumped"," punches "," punish ","punt road"," pupils "," pursue ",
                                " put down "," puts on the jet",
                            "quality ","quarter"," questions ","quick hands","quick maths","quick thinking","quick response","quick thought"," quietly"," quotes ",
                            " racism "," racist "," radar "," radio"," rampant"," ranked "," ranks ","rate this "," ratings "," rattle ",
                                " reached out "," reaction ", " read that "," ready ","ready to go","real deal"," really "," reality check"," reasonable",
                                "recommended"," record ","recruiter","red bull "," refugee "," regional ",
                                " rehabilitation ","relishing",
                                " remainder "," remain unbeaten"," rematch "," remember","remember "," repeat."," repeat*"," reported "," reported."," requested ",
                                " resigned ","re-signed"," resolution"," respect "," responds "," responsibility ", "rest of the match",
                                "retirement","return to face"," reunion."," rev ",
                                " riding "," right direction"," right now"," ripper","rising star",
                                "rivalry", " rivals ",
                                "roadrunner","road runner", "roaring ","roasted "," rocket "," rocking "," roof will","rollercoaster"," rolling "," room "," rooms "," roost"," roster ",
                                "rough conduct","rough contact",
                                "round six ","round eight ","round nine ","round ten","round 14 ", "round 15 ", "round 16 ", "round 17 ", "round 18 ","round 19 ","round 20 ","round 21 ","round 22 ","round 23 ",
                                "rounding out " ,"round so far" ,"round the corner" ," routine " ,"rovers "," roves "," roving ","roving 101",
                                " rue "," rugby "," run and jump "," runner ","running ","run-with",
                             "sacrifice "," salutes "," salary "," says no"," say too much",
                                "scans have revealed"," scars "," scent "," schedule "," school "," scooter ","scoreboard","scoresheet","scoring",
                                " scalp "," scalps "," scathing","scrappy"," screamer",
                                //" season",
                                " seat ","second half","second-half"," seeking"," see something"," selfless ","sells the candy",
                                " senior "," sentiment "," sensational ",
                                "serious"," serves ","serving ", " session "," set shot"," sets sail ","set to go ",
                                "seven for ","sevens-vfl","sevens-waf", " several ",
                                "shades of ","shadows ","sheeeeeesh","shocked"," shone "," shook up "," shoot,"," shot "," showdown ","showing off","shows his class",
                                "sidelined","sidestep","signhim",
                                //" signing ",
                                " signs off as "," silence"," silent"," simple "," simpler "," sinker "," siren",
                                " skies ",
                                "slammed ","sliders ","slides it", "sliding ","slip over","slippery "," slot "," slots "," sluggish "," slump",
                                " smart tap "," smile "," smiles ",
                                "smooth "," smother ",
                                " snag "," snap "," snaps "," snaps."," snatches ","sneak ","sneaks ","snooze ",
                                " soaking ", " soars high"," soccer ","socceroos","soccerroos"," soccers ","so far:","so good"," sold out "," solidifying "," some finish "," some light "," sorry.",
                                "spaceman"," speak ","speaking to "," speaks ","speaks out","speaks with","spearhead","special","spectacular"," speculation ",
                                " spice."," spicy ","spin cycle","spinning","spins and finish"," spirit "," splits ",
                                "spoke to ","sportsbet"," sportsmanship","sportswomen","spotify","spotlight"," sprint ",
                                "ssss",
                                "staggering","stand for that","stanza",
                                " starred "," stars."," started:"," starts here","statement ","state of origin","statistics"," stays down ","stay strong","stay tuned",
                                " steadies"," steam "," stepped "," steps up "," story ","story:","story | "," storyline","stoppage",
                                " straight ","-straight "," streak "," strange "," strength."," stretch."," stretcher","stretchered off",
                                //" strikes ", // worker strikes cause match time to change 
                                " strong "," structured ",
                                " stuff "," stuffed "," stunned ",
                                //"subbed in ","subbed off","subbed on","subbed out",
                                " submit ","subscriber"," success","sufficient"," suits you "," sums it up","sunderlandafc","sunshine state",
                                "superb","supercars","superhero","superstar","supportive",
                                "surely can't","surely not"," surge "," surged ",
                                //" surgery",
                                " surgeon"," surprise"," survey "," survive "," suspensions ",
                                " swapping picks"," swarming","sweet start"," sweat ","sweating"," swim ","swimmer","swimming"," swipe ",
                                "swoop "," swoops "," sword",
                                "symbolised","synergy",
                            "tackling","take a bow ","take a look back","take the silver","takes a look","taking over"," taliban "," talking "," talks us "," target "," targeting ",
                                "tasmania jumper","tasstateleague",
                                " teach ",
                                "team in front","team of the year","technically","technician","technology","tennis"," tense "," tense."," tension"," tenure ","terrific",
                                "test captain","test gets underway","test match","test wicket","tex!","textbook","texting",
                                "thank ","thanks"," thanos ","that is all","that's three for",
                                "the action","the ball ","the bench ","the fly ","the gap ","the guy ","the hard way","the ladder", "themselves", "therabody","the reward","the round",
                                "the stage "," the trip","the wash up","the win"," things "," think ","third fifty",
                                "this effort","this guy",
                                //"this year",
                                " thorns "," thought "," thoughts "," thread "," threads ","threepeat","thriller","thrilling","thumbs up",
                                " tick ","ticketing","ticket price","tickets","tidy finish","tigerland"," tillies",
                                "toe-poke"," to go.","tomahawk","too close","too good","too soon",
                                //"top eight","top four",
                                "top job","top tier","torched","to the wire","to this point"," totw ",
                                " touch "," touched "," touches "," tough ","tough kick",
                                " trade "," traded ","trademark","traderadio","trade request"," trading","traffic",
                                " trainers","transcribe","transition"," trauma ",
                                " treasure"," treat."," treatment ","trend-setter","triathlon",
                                //"tribunal",
                                //"tribute",
                                " trick "," tricks."," trophy"," troublesome ",
                                " tsunami ",
                                " tumbling "," tumultuous", " tune in ", "tune into "," turf "," turnaround"," turnover",
                                "tweaks"," tweet.",
                                //"twitter",
                                "two in a minute ",
                            " u16 ","ultimate "," umpire ", " umpiring ",
                                "unbelievable",
                                "undefeated","under 18","understanding","understatement","under the ","underway","unfortunately",
                                "unique achievement"," uniting ",
                                //" unleash",
                                "unpack"," untenable "," unturned ",
                                "up and about","up his sleeve"," upset ",
                                "usafl","us apart","us open","u-turn",
                            "vaccinated","vale, "," variety "," veins ","versatile"," version "," very good ", 
                                //" vfl ",
                                "vflw",
                                " vibes "," vibes."," vicious"," victory."," video ","videos"," vile ","vintage "," violation","virgin "," vision ",
                                " voiced ", " vote "," voted "," votes"," voting ",
                            "wafl","wallabies","wantaway"," wanting "," watching "," watch the ","warmin'","way back","ways to stop","wayward","wbbl",
                                "we are set"," weaves ","websites"," week one ","weighing","weighs up ","weightlifter","welcome ","well and truly"," went at it ","westminster abbey",
                                "what a ","whatafeeling","what he does.","what just happen","what lies ","wheels",
                                "whisperer","whiteboard","white flag","wholesome","who would you","why not ",
                                "wide-open"," wife "," will unite ","win for the ","winners","winning","wishes ","wishing ",
                                " with ease",
                                " within "," with space"," wizard ","wnba","wnbl",
                                "woah "," woman "," women ","women's","womens:","womens.afl","womens ","wonderful","wonder what",
                                " wooden spoons"," woops "," words "," work ","world cup","worldcup","world record","world doping"," worried ","worth noting"," worthy "," wouldn't ","would you make"," wounded ","#wow"," wow.",
                                " wrapped ","wrapping",
                            " yarn "," years ","yesterday","yokayifooty","you know the rest"," youngsters "," your best "," you see me "



                        };

        // emojis
        // 🎁🎁🎁
        // s1 = "🎁🎁🎁";
        // var ExcludeHex = new[] { };
        //                        var ExcludeHex = new[] { "F09F8E81F09F8E81F09F8E81" };
        // https://stackoverflow.com/questions/8727146/how-do-i-initialize-an-empty-array-in-c

        // https://github.com/Microsoft/microsoft-ui-xaml
        // re accessing notifs 
        // 
        // WinUI 3(Q4 2019 - 2020)
        // https://github.com/microsoft/microsoft-ui-xaml/blob/master/docs/roadmap.md
        // Interop: use WinUI 3 to extend existing WPF, WinForms and MFC apps with modern Fluent UI

        //var notifier = ToastNotificationManager.CreateToastNotifier();
        //var notifications = notifier.GetScheduledToastNotifications();
        //s1=notifications.Count().ToString(); // 0
        // empty now as can just include emojis as strings, but the above is the hex for those emojis
        public string[] ExcludeHex = new string[0]; 
        public bool IsRelevant(NotificationItem un)
        {
            // copied from UWP
            // Examine a UserNotification to see whether it is required  

            try
            {
                string Source = un.Source ?? "unknown";
                string bodyText = un.Body ?? "";
                string titleText = un.Title ?? "";
                uint ID = un.ID;
                bool RequiredTweet = false;
                bool ForcedNotification = false;

                string FilterReason = "";
                string FilterValue = ""; // 20210926

                // 20210329 - always output this
                Log.Information("Parsing UserNotification with ID={id},CreationTime={ct},Source={ai},titleText={tt},bodyText={bt}", un.ID, un.CreationTime.ToString("yyyy-MM-dd HH':'mm':'ss"), Source, titleText, bodyText);

                // 20200529 and added filtering check on 20200623
                //ForcedNotification = ActiveNotifications.Count < TweetMinimum || !NotificationFiltering;

                // 20210329
                // stopped checking title text here as sometimes it might be adelaide crows perhaps
                bool exclApp = (ExcludeApps.Any(Source.ToLower().Contains));
                bool inclApp = (IncludeApps.Any(Source.ToLower().Contains));
                bool exclTitle = (ExcludeTitle.Any(titleText.ToLower().Contains));
                bool inclTitle = (IncludeTitle.Any(titleText.ToLower().Contains));
                int MinMsgLength = 20;

                if (!ForcedNotification)
                {
                    // 20210601
                    // Extract urls and hashtags

                    RemoveExcessText(ID, ref bodyText);

                    if (bodyText.Length < MinMsgLength)
                    {
                        RequiredTweet = false;
                        FilterReason = "Trimmed Message body length < " + MinMsgLength.ToString() + " it is " + bodyText.Length.ToString();
                        FilterValue = bodyText;
                    }

                    else if (exclApp)
                    {
                        RequiredTweet = false;
                        FilterReason = "Excluded application";
                        FilterValue = Source;
                    }
                    else if (ExcludeEmojis.Any(titleText.ToLower().Contains))
                    {
                        RequiredTweet = false;
                        FilterReason = "Excluded title since it has an excluded emoji in it";
                        FilterValue = ExcludeEmojis.First(titleText.ToLower().Contains);
                    }
                    else if (inclApp)
                    {
                        // 20210421

                        if (exclTitle)
                        {
                            RequiredTweet = false;
                            FilterReason = "Excluded Title";
                            FilterValue = ExcludeTitle.First(titleText.ToLower().Contains);
                        }
                        else if (inclTitle)
                        {
                            // check for emojis
                            // https://stackoverflow.com/questions/16999604/convert-string-to-hex-string-in-c-sharp

                            byte[] ba = Encoding.Default.GetBytes(bodyText);
                            var hexString = BitConverter.ToString(ba).Replace("-", "");

                            // 20210413
                            // Cricket Australia sends tweets as a wicket falls with title = player name and text = blank
                            // 
                            if (bodyText.Trim().Length == 0)
                            {
                                RequiredTweet = false;
                                FilterReason = "Blank bodyText";
                            }
                            else if (IncludeBodyValues.Any(bodyText.ToLower().Contains))
                            {
                                if (IncludeBodyValuesOverrides.Any(bodyText.ToLower().Contains))
                                {
                                    RequiredTweet = false;
                                    //s1 = "IncludeBodyValues tweet detected =[" + bodyText + "]";
                                    //Log.Debug(s1);
                                    FilterReason = "Include body Values overriden";
                                    FilterValue = IncludeBodyValuesOverrides.First(bodyText.ToLower().Contains);
                                }
                                else
                                {
                                    RequiredTweet = true;
                                    //s1 = "IncludeBodyValues tweet detected =[" + bodyText + "]";
                                    //Log.Debug(s1);
                                    FilterReason = "Include body Values not overridden";
                                    FilterValue = IncludeBodyValues.First(bodyText.ToLower().Contains);
                                }
                            }
                            else if (ExcludeStartValues.Any(bodyText.ToLower().StartsWith))
                            {
                                RequiredTweet = false;
                                FilterReason = "ExcludeStartValues";
                                FilterValue = ExcludeStartValues.First(bodyText.ToLower().StartsWith);
                            }

                            else if (ExcludeBodyValues.Any(bodyText.ToLower().Contains))
                            {
                                RequiredTweet = false;
                                FilterReason = "ExcludeBodyValues";
                                // 20210629
                                // could just use first always in each boolean test rather than .any but needs a rewrite of all the if elses etc here
                                // for now test this way - shouldn't be huge oveerhead
                                //s1 = ExcludeBodyValues.FirstOrDefault(bodyText.ToLower().Contains);
                                FilterValue = ExcludeBodyValues.First(bodyText.ToLower().Contains);
                                //Log.Information(FilterValue);
                            }
                            else if (ExcludeEmojis.Any(bodyText.ToLower().Contains))
                            {
                                RequiredTweet = false;
                                FilterReason = "ExcludeEmojis";
                                FilterValue = ExcludeEmojis.First(bodyText.ToLower().Contains);
                            }
                            else if (ExcludeHex.Any(hexString.Contains))
                            {
                                RequiredTweet = false;
                                FilterReason = "ExcludeHex";
                                FilterValue = ExcludeHex.First(hexString.Contains);
                            }
                            else
                            {
                                RequiredTweet = true;
                                FilterReason = "NotExcluded by anything";
                                // Debug.WriteLine(DateTime.Now.ToString() + " - else tweet=[" + bodyText + "]");
                            }

                        }
                        else
                        {
                            RequiredTweet = false;
                            FilterReason = "Not an Excluded Title, but not an included title";
                            // 20210713
                            FilterValue = titleText;
                        }
                    }
                    else
                    {
                        FilterReason = "Source is not excluded, but is not on included Apps list";
                        // 20210713
                        FilterValue = Source;
                    }
                }
                else
                {
                    RequiredTweet = true;
                    FilterReason = "ForcedNotification";
                }


                if (RequiredTweet)
                {

                    Log.Information("A candidate RequiredTweet (Forced = {f}) was detected with id={o},body={st},bodylen={bl}. FilterReason={f} Filtervalue={v}",
                                    ForcedNotification, ID, bodyText, bodyText.Length, FilterReason, FilterValue);  // 20210629

                    // https://stackoverflow.com/questions/23207664/how-to-bind-an-observablecollection-to-an-sql-table
                    // https://stackoverflow.com/questions/28422800/update-itemscontrol-when-an-item-in-an-observablecollection-is-updated?rq=1
                    // An observable collection is observing the collection itself, not properties of the children.
                    // https://stackoverflow.com/questions/14179967/fill-observablecollection-directly-from-sql-server
                    // https://docs.microsoft.com/en-us/dotnet/desktop-wpf/data/data-binding-overview#binding-to-collections
                    // To improve performance, collection views for ADO.NET DataTable or DataView objects delegate sorting and filtering to the DataView, which causes sorting and filtering to be shared across all collection views of the data source. 
                    // https://docs.microsoft.com/en-us/dotnet/framework/wpf/data/how-to-bind-to-an-ado-net-data-source

                    // 20210413
                    // Exclude dups that have different ids but are otherwise the same

                    bool DuplicateDetected = false;
                    //CheckForDuplicateNotification(ActiveNotifications, ID, titleText, bodyText, ref FilterReason, ref DuplicateDetected);
                   
                    if (!DuplicateDetected)
                    {
                        // This Notif isn't in the vm yet with this id
                        //Log.Debug("The RequiredTweet id={o} is not already on the vm as an ActiveNotification", ID);// 20200925

                        if (VisibilityDefault >= NotifVisibilityThreshold)
                        {
                            // 20200715
                            // Only add it to Active if the thresh is 6 or less = add only when it is active 

                            // add the notification to the vm and model since it isn't already in the vm

                            //var sn = new ActiveNotification();
                            //sn.Id = (int)un.Id;

                            //// https://docs.microsoft.com/en-us/dotnet/standard/datetime/converting-between-datetime-and-offset

                            //sn.CreationTime = un.CreationTime.DateTime;
                            //sn.Source = Source;
                            //sn.Title = titleText;
                            //sn.Body = bodyText;
                            //// ts is defined as auto in SQL Server? nope, not working
                            //sn.TS = DateTime.Now;
                            //sn.Visibility = VisibilityDefault; // 20200617 = default

                            //s1 = "New Active Notification candidate detected. ID=" + sn.Id.ToString() + " ,Title= " + sn.Title + " ,Body= " + sn.Body + " It will be added to ActiveNotifications in vm";
                            //Log.Information(s1);

                            //// 20200925
                            //UpdateActiveNotifications(sn);
                            //SetSelectedNotification();

                            //Log.Information("A new notification with ID={id} was added to ActiveNotifications and it will now be written to the model/database ", sn.Id);

                            //// 20200602
                            //// Also add it to model/database
                            //WriteNotification(sn);

                            // 20200721
                            // https://www.finalsiren.com/PlayerCompare.asp?SeasonID=&Compare=Compare&PlayerName1=Brennan+Cox&PlayerName2=Taylin+Duman&PlayerName3=&PlayerName4=&SelectedPlayers=
                            // hangs
                            // but if it does eventually works, this may be the way to get team? and player ids from FS and then use the IDs to GET player ratings when late changes occur.
                            // may not even need to store the DS keys on my database. Just get it via web request each time and store the current rating on the teamchange table or whatever the table that holds Notes is/will be called.


                            // https://stackoverflow.com/questions/38103634/how-do-i-popup-in-message-in-uwp
                            // https://stackoverflow.com/questions/22909329/universal-apps-messagebox-the-name-messagebox-does-not-exist-in-the-current

                            //var xs = new StringBuilder();
                            //xs.
                            // 20200622 
                            // on load got
                            // Exception from HRESULT: 0x800403E9
                            // from MessageDialog call below
                            // https://docs.microsoft.com/en-us/openspecs/windows_protocols/ms-erref/705fb797-2175-4a90-b5a3-3918024b10b8
                            // code not documented
                            // https://docs.microsoft.com/en-us/openspecs/windows_protocols/ms-erref/1bc92ddf-b79e-413c-bbaa-99a5281a6c90
                            // not here either nor any 800403xx errors

                            // also 
                            // https://stackoverflow.com/questions/15943682/exception-from-hresult-0x8001010e-rpc-e-wrong-thread
                            // 
                            //   System.Exception: The application called an interface that was marshalled for a different thread. (Exception from HRESULT: 0x8001010E (RPC_E_WRONG_THREAD))
                            //at Windows.UI.Xaml.Application.get_Resources()
                            //at UWPExampleCS.MainPage.MainPage_obj1_Bindings.LookupConverter(String key)
                            //at UWPExampleCS.MainPage.MainPage_obj1_Bindings.Update_ViewModel_ActiveNotifications_Count(Int32 obj, Int32 phase)
                            //at UWPExampleCS.MainPage.MainPage_obj1_Bindings.MainPage_obj1_BindingsTracking.PropertyChanged_ViewModel_ActiveNotifications(Object sender, PropertyChangedEventArgs e)

                            // maybe from
                            //   ActiveNotifications.Add(sn); above??

                            // https://stackoverflow.com/questions/3772233/win32-setforegroundwindow-unreliable?noredirect=1&lq=1
                            // Once you have the handle for the window, you can simply call:
                            // SetForegroundWindow(handle);
                            // https://stackoverflow.com/questions/39451928/put-application-to-foreground



                            //bool ShowDialog = false;
                            //ContentDialog NotifDialog = null;

                            // If ShowAsync is done on the notification content dialog, when the app is launched below, the app icon in the taskbar doesn't flash for attention
                            // If ShowAsync is NOT done, the app icon in the taskbar DOES flash when the app is launched below.

                            // THIS IS THE BEST I CAN DO SO FAR

                            //if (ShowDialog)
                            //{

                            //    // 20200721
                            //    // https://docs.microsoft.com/en-us/windows/uwp/design/controls-and-patterns/dialogs-and-flyouts/dialogs

                            //    NotifDialog = new ContentDialog
                            //    {
                            //        Title = "New Important Tweet",
                            //        Content = s1,
                            //        CloseButtonText = "Ok",
                            //        HorizontalAlignment = HorizontalAlignment.Left, // seems to be overridden by FullSizeDesired
                            //        Width = Window.Current.Bounds.Width  // seems to be overridden by FullSizeDesired
                            //                                             // FullSizeDesired =true // makes this dialog centered and as tall as the main app window 
                            //    };
                            //    ContentDialogResult result = await NotifDialog.ShowAsync(ContentDialogPlacement.InPlace);
                            //}


                            // https://docs.microsoft.com/en-us/uwp/api/Windows.UI.Xaml.Controls.ContentDialog?view=winrt-19041

                            // Only one ContentDialog can be shown at a time. To chain together more than one ContentDialog, handle the Closing event of the first ContentDialog. 
                            // In the Closing event handler, call ShowAsync on the second dialog to show it.
                            // https://docs.microsoft.com/en-us/uwp/api/windows.ui.xaml.controls.contentdialog.showasync?view=winrt-19041#Windows_UI_Xaml_Controls_ContentDialog_ShowAsync_Windows_UI_Xaml_Controls_ContentDialogPlacement_
                            // ContentDialogPlacement.InPlace not much use but tested anyway
                            // https://stackoverflow.com/questions/33732210/set-contentdialog-to-show-on-bottom-of-page-in-uwp
                            // It seems the positioning of ContentDialog instances is not in the hands of the developer, at least not without custom versions of it.
                            // https://stackoverflow.com/questions/49504479/doing-work-while-a-contentdialog-is-open

                            // https://stackoverflow.com/questions/34755360/uwp-detect-app-gaining-losing-focus
                            // On a Windows 10 PC in desktop mode, however, it will mean whenever your app is minimized. 
                            // ******************************!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                            // https://stackoverflow.com/questions/42176570/bring-uwp-app-in-foreground
                            // ********************************************************
                            // https://stackoverflow.com/questions/39451928/put-application-to-foreground


                            // https://docs.microsoft.com/en-us/previous-versions/windows/apps/hh464906(v=win.10)?redirectedfrom=MSDN#background_tasts
                            // Apps use contracts and extensions to declare the interactions that they support with other apps. 
                            // These apps must include required declarations in the package manifest and call required APIs to communicate with other contract participants.

                            // 20200722
                            // https://stackoverflow.com/questions/42176570/bring-uwp-app-in-foreground
                            // In Package.appxmanifest, I created a declaration of type Protocol with a name of uwptoforeground
                            // and Desired View of Use More

                            // https://docs.microsoft.com/en-us/windows/uwp/launch-resume/handle-uri-activation
                            // (UWP) apps can register to be a default handler for a URI scheme name. 
                            // your app will be activated every time that type of URI is launched.

                            // Well, it activates the app ok and initialises the app ok, but it doesn't actually bring it to the foreground in Win 10.
                            // Also tried this with no notification dialog above.
                            // In both cases, the app's icon in the taskbar flashes but the app isn't in the foreground.

                            // If I do not launch it, we don't even get the icon flashing, so at least this helps a bit 

                            // So, this doesn't achieve the aim of having app in the foreground, but it at least causes the taskbar icon to flash, so that is an improvement, so I may as well launch it.


                            //bool LaunchApp = true;

                            //if (LaunchApp)
                            //{
                            //    // https://docs.microsoft.com/en-us/windows/uwp/launch-resume/launch-default-app
                            //    // Set the desired remaining view.
                            //    // overrides what is defined in the manifest

                            //    var options = new Windows.System.LauncherOptions();
                            //    options.DesiredRemainingView = Windows.UI.ViewManagement.ViewSizePreference.UseMore; // use more/less window space for current launching app than the target app
                            //                                                                                         // eg launching a mail app 
                            //                                                                                         // but in my case they are the same app?? this doesn't seem to do anything when the dialog is used

                            //    Log.Information("About to launch App via uwptoforeground: protocol with options.DesiredRemainingView={d}", options.DesiredRemainingView);
                            //    await Windows.System.Launcher.LaunchUriAsync(new Uri("uwptoforeground://"), options);

                            //    Log.Information("App has been launched via uwptoforeground: protocol");
                            //}

                            //// https://docs.microsoft.com/en-us/uwp/api/windows.ui.xaml.controls.control.focus?view=winrt-19041#Windows_UI_Xaml_Controls_Control_Focus_Windows_UI_Xaml_FocusState_
                            //// also tried doing this before the launch - no joy
                            //if (!(NotifDialog is null))
                            //{
                            //    bool focusState = NotifDialog.Focus(FocusState.Programmatic); // does nothing??

                            //    if (focusState)
                            //    {
                            //        Log.Information("Focus was already on or was set to the ContentDialog");
                            //    }
                            //    else
                            //    {
                            //        Log.Information("Focus could not be set to the ContentDialog"); // this is true, even if app launched
                            //    }
                            //}


                            // In desktop mode, note that even when another app window covers up your app window, your app is still considered by the UWP app lifecycle to be running in the foreground.

                            //await new MessageDialog(s1).ShowAsync();

                            // 20200601
                            // output this one to tbox1 etc. on the page - this is obs really as I now use the tboxes to hold the current/selected notif
                            // keep as it holds an example of getlogo
                            //ShowCurrentNotification(un, Source, titleText, bodyText);
                        }

                    }
                    else
                    { // This Notif is in the vm already with matching id or title/body
                        // 20200925
                        Log.Information("The RequiredTweet id={o} is already on the vm as an ActiveNotification. Reason={r}. It won't be added again", ID, FilterReason);

                    }

                }
                else // !RequiredTweet
                {
                    Log.Information("A NON RequiredTweet id={o} was detected. It is being skipped for FilterReason={f}, FilterValue={v},bodyText={b}",
                             ID, FilterReason, FilterValue, bodyText);

                    //if (ActiveNotifications.Any(x => x.Id == un.Id))
                    //{
                    //    Log.Information("A NON RequiredTweet id={o} is already on the VM as an ActiveNotification but is being skipped anyway as it is not required.", ID);// 20200925}
                    //}
                    //else
                    //{
                    //    Log.Information("A NON RequiredTweet id={o} was detected. It is NOT already on the vm as an ActiveNotification. It is being skipped for FilterReason={f}, FilterValue={v},bodyText={b}",
                    //         ID, FilterReason, FilterValue, bodyText);
                    //    // 20200925 - this might be where ListView gets out of sync with vm/datagrid??  maybe notif exists on db / was forced?
                    //}

                }

                // https://docs.microsoft.com/en-us/windows/uwp/design/shell/tiles-and-notifications/notification-listener

                //if (DeletedNotifs < DeleteOldestNotifications)
                //{
                //    listener.RemoveNotification(un.Id);
                //    DeletedNotifs++;
                //}

                return RequiredTweet;

            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                //StatusMessage = ex.Message;
                return false;
                //throw;
            }
        }
        private void RemoveExcessText(uint id, ref string bodyText)
        {
            // 20210601

            // example of what we do not want

            // FYFE
            // #AFLPowerFreo https://t.co/urSxmaVs3A

            // for now just remove hashtags and urls 

            // 20210623
            // keep hashtags as they are used in late changes
            try
            {

                string MainBody;

                bool RemoveHashTags = false;

                Log.Debug("Raw body Input for ID={is} is [{f}] with length of {l}", id, bodyText, bodyText.Length);

                // extract the hashtags and urls
                // https://stackoverflow.com/questions/19205107/operator-cannot-be-applied-to-operands-of-type-char-and-string
                // https://stackoverflow.com/questions/5031726/linq-where-vs-takewhile
                // https://stackoverflow.com/questions/41543605/get-the-part-of-a-string-before-new-line-symbol
                // https://stackoverflow.com/questions/1563844/best-hashtag-regex

                // https://stackoverflow.com/questions/6155219/c-sharp-regex-issue-unrecognized-escape-sequence

                MainBody = bodyText;

                if (RemoveHashTags)
                {
                    string regExp = @"(?<=\s|^)#(\w*[A-Za-z_]+\w*)";
                    MainBody = Regex.Replace(MainBody, regExp, "");
                    Log.Information("For ID={is} body after hashtag removal is [{f}] with length of {l}", id, MainBody, MainBody.Length);
                }

                // thanks to @sportsbetcomau

                // https://stackoverflow.com/questions/27156914/regex-to-remove-urls-from-string-c-sharp

                MainBody = Regex.Replace(MainBody, @"((([A-Za-z]{3,9}:(?:\/\/)?)(?:[-;:&=\+\$,\w]+@)?[A-Za-z0-9.-]+|(?:www.|[-;:&=\+\$,\w]+@)[A-Za-z0-9.-]+)((?:\/[\+~%\/.\w-_]*)?\??(?:[-\+=&;%@.\w_]*)#?(?:[\w]*))?)",
                                         string.Empty);
                Log.Debug("For ID={is} body after url removal is [{f}] with length of {l}", id, MainBody, MainBody.Length);

                bodyText = MainBody;

            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                //StatusMessage = ex.Message;
                throw;
            }
        }


    }
}
