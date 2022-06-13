using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
#if UNITY_ANDROID
using Unity.Notifications.Android;
#elif UNITY_IOS
using Unity.Notifications.iOS;
#endif

namespace BP
{
    public struct NotificationContent
    {
        public int id;
        public string iconId;
        public string title;
        public string message;
    }

    public struct NotificationTime
    {
        public int day;
        public System.TimeSpan time;
    }

    public class NotificationController : Singleton<NotificationController>
    {
        #region Constants
        private const string CHANNEL_ID = "bpj_1010_notification_channel_0";
        private const string LARGE_ICON_ID = "large_icon_id";
        #endregion

        #region Events
        #endregion

        #region Fields
        #endregion

        #region Properties
        private List<NotificationContent> m_Contents = new List<NotificationContent>();
        private List<NotificationTime> m_Times = new List<NotificationTime>();

        private bool m_IsNotificationGranted = false;
        private string m_DeviceToken;
        #endregion

        #region Unity Events
        private void Start()
        {
            FetchRemoteNotificationContents();
            FetchRemoteNotificationTime();

#if UNITY_IOS
            StartCoroutine(RequestAuthorization());
#endif
        }
        #endregion

        #region Methods
        public void SetupNotificationsWithDelay(float delay)
        {
            StartCoroutine(DoSetupNotificationWithDelay(delay));
        }

        private IEnumerator DoSetupNotificationWithDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
#if UNITY_ANDROID
            var c = new AndroidNotificationChannel()
            {
                Id = CHANNEL_ID,
                Name = "Block Puzzle Channel",
                Importance = Importance.High,
                Description = "Generic notifications",
                LockScreenVisibility = LockScreenVisibility.Private,
                CanShowBadge = true,
                EnableLights = true,
                EnableVibration = true,
            };
            AndroidNotificationCenter.RegisterNotificationChannel(c);

            var notificationIntentData = AndroidNotificationCenter.GetLastNotificationIntent();
            if (notificationIntentData != null)
            {
                TrackOpenAppFromNotification();
            }

            AndroidNotificationCenter.CancelAllDisplayedNotifications();
            AndroidNotificationCenter.CancelAllScheduledNotifications();
#elif UNITY_IOS
            var notification = iOSNotificationCenter.GetLastRespondedNotification();
            if (notification != null)
            {
                TrackOpenAppFromNotification();
            }

            iOSNotification[] notifications = iOSNotificationCenter.GetScheduledNotifications();
            Logger.d("Get scheduled notifications: ", notifications.Length);
            for (int i = 0; i < notifications.Length; i++)
            {
                iOSNotificationCalendarTrigger time = (iOSNotificationCalendarTrigger)notifications[i].Trigger;
                Logger.d("Notification ", notifications[i].Identifier, "\nTitle: ", notifications[i].Title, "\nTime: ", time.Year, "/", time.Month, "/", time.Day, " - ", time.Hour);
            }

            iOSNotificationCenter.RemoveAllDeliveredNotifications();
            iOSNotificationCenter.RemoveAllScheduledNotifications();
#endif

            yield return new WaitForSeconds(1f);
            SetupNotifications();
        }

        public void SetupNotifications()
        {
            if (m_Contents.Count == 0 || m_Times.Count == 0)
                return;

#if UNITY_IOS
            if (!m_IsNotificationGranted)
            {
                Logger.d("Notification on this device is not granted!");
                return;
            }
#endif

            for (int i = 0; i < m_Times.Count; i++)
            {
                NotificationContent randomContent = m_Contents[Random.Range(0, m_Contents.Count)];

                NotificationTime timeData = m_Times[i];
                System.DateTime deliveryTime = System.DateTime.Now.ToLocalTime().AddDays(timeData.day);
                deliveryTime = new System.DateTime(deliveryTime.Year, deliveryTime.Month, deliveryTime.Day, timeData.time.Hours, timeData.time.Minutes, timeData.time.Seconds,
                    System.DateTimeKind.Local);
                
                // Logger.d("Send notification: Id: ", randomContent.id, " on time: ", deliveryTime.ToString("ddd, dd MMM yyy HH’:’mm’:’ss ‘GMT’"));
                SendNotification(randomContent.title, randomContent.message, deliveryTime, 0, false, CHANNEL_ID, randomContent.iconId, LARGE_ICON_ID);
            }
        }

        #if UNITY_IOS
        private IEnumerator RequestAuthorization()
        {
            var authorizationOption = AuthorizationOption.Alert | AuthorizationOption.Badge;
            using(var req = new AuthorizationRequest(authorizationOption, true))
            {
                while (!req.IsFinished)
                {
                    yield return null;
                };

                string res = "\n RequestAuthorization:";
                res += "\n finished: " + req.IsFinished;
                res += "\n granted :  " + req.Granted;
                res += "\n error:  " + req.Error;
                res += "\n deviceToken:  " + req.DeviceToken;
                Logger.d(res);

                m_IsNotificationGranted = req.Granted;
                m_DeviceToken = req.DeviceToken;
            }
        }
        #endif

        private void TrackOpenAppFromNotification()
        {
            Logger.d("Open app from notification!!!");
        }

        /// <summary>
        /// Queue a notification with the given parameters.
        /// </summary>
        /// <param name="title">The title for the notification.</param>
        /// <param name="body">The body text for the notification.</param>
        /// <param name="deliveryTime">The time to deliver the notification.</param>
        /// <param name="badgeNumber">The optional badge number to display on the application icon.</param>
        /// <param name="reschedule">
        /// Whether to reschedule the notification if foregrounding and the notification hasn't yet been shown.
        /// </param>
        /// <param name="channelId">Channel ID to use. If this is null/empty then it will use the default ID. For Android
        /// the channel must be registered in <see cref="GameNotificationsManager.Initialize"/>.</param>
        /// <param name="smallIcon">Notification small icon.</param>
        /// <param name="largeIcon">Notification large icon.</param>
        private void SendNotification(string title, string body, System.DateTime deliveryTime, int? badgeNumber = null,
            bool reschedule = false, string channelId = null,
            string smallIcon = null, string largeIcon = null)
        {

#if UNITY_ANDROID
            var notification = new AndroidNotification();
            notification.Title = title;
            notification.Text = body;
            notification.FireTime = deliveryTime;
            notification.Group = !string.IsNullOrEmpty(channelId) ? channelId : CHANNEL_ID;
            notification.SmallIcon = smallIcon;
            notification.LargeIcon = largeIcon;
            notification.Color = new Color32(0, 87, 231, 255);

            AndroidNotificationCenter.SendNotification(notification, CHANNEL_ID);
#elif UNITY_IOS
            var calendarTrigger = new iOSNotificationCalendarTrigger()
            {
                Year = deliveryTime.Year,
                Month = deliveryTime.Month,
                Day = deliveryTime.Day,
                Hour = deliveryTime.Hour,
                Minute = deliveryTime.Minute,
                Second = deliveryTime.Second,
                Repeats = false
            };

            var notification = new iOSNotification()
            {
                Title = title,
                Body = body,
                CategoryIdentifier = "category_tony_block_puzzle",
                ThreadIdentifier = "tony_block_puzzle_thread1",
                Trigger = calendarTrigger,
            };

            iOSNotificationCenter.ScheduleNotification(notification);
#endif
        }

        private void FetchRemoteNotificationContents()
        {
            string contentString = PrefsUtils.GetString(Consts.PREFS_NOTIFICATION_CONTENTS, "");
#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(contentString))
            {
                string filePath = "Assets/Resources/notification_contents.json";
                using(System.IO.FileStream fs = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
                {
                    using(System.IO.StreamWriter writer = new System.IO.StreamWriter(fs))
                    {
                        writer.Write(contentString);
                    }
                }
            }
#endif

            if (string.IsNullOrEmpty(contentString))
            {
                contentString = Resources.Load<TextAsset>("notification_contents").ToString();
                PrefsUtils.SetString(Consts.PREFS_NOTIFICATION_CONTENTS, contentString);
                PrefsUtils.Save();
            }

            if (!string.IsNullOrEmpty(contentString))
            {
                var rootJson = JSON.Parse(contentString);
                JSONArray contents = rootJson["contents"].AsArray;
                for (int i = 0; i < contents.Count; i++)
                {
                    JSONNode contentNode = contents[i];
                    NotificationContent content = new NotificationContent()
                    {
                        id = contentNode["id"].AsInt,
                        title = contentNode["title"].Value,
                        message = contentNode["message"].Value,
                        iconId = contentNode["small_icon"].Value
                    };
                    m_Contents.Add(content);
                }
            }
        }

        private void FetchRemoteNotificationTime()
        {
            string contentString = PrefsUtils.GetString(Consts.PREFS_NOTIFICATION_TIMES, "");
#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(contentString))
            {
                string filePath = "Assets/Resources/notification_times.json";
                using(System.IO.FileStream fs = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
                {
                    using(System.IO.StreamWriter writer = new System.IO.StreamWriter(fs))
                    {
                        writer.Write(contentString);
                    }
                }
            }
#endif

            if (string.IsNullOrEmpty(contentString))
            {
                contentString = Resources.Load<TextAsset>("notification_times").ToString();
                PrefsUtils.SetString(Consts.PREFS_NOTIFICATION_TIMES, contentString);
                PrefsUtils.Save();
            }

            if (!string.IsNullOrEmpty(contentString))
            {
                var rootJson = JSON.Parse(contentString);
                JSONArray contents = rootJson["times"].AsArray;
                for (int i = 0; i < contents.Count; i++)
                {
                    int dayNext = contents[i]["day"].AsInt;
                    string timeString = contents[i]["time"].Value;
                    System.TimeSpan timeSpan;
                    try
                    {
                        timeSpan = System.TimeSpan.Parse(timeString);
                    }
                    catch (System.Exception exc)
                    {
                        Debug.LogException(new System.Exception(string.Format("Remote parse time error: {0}", exc.Message)));
                        Logger.e("Remote parse time error: ", exc.Message, " \nUse default time!");
                        timeSpan = System.TimeSpan.Parse("11:00:00");
                    }
                    m_Times.Add(new NotificationTime()
                    {
                        day = dayNext,
                            time = timeSpan
                    });
                }
            }
        }
        #endregion
    }
}