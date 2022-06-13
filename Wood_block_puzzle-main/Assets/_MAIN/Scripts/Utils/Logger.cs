using System.Collections;
using System.Collections.Generic;
using BP;
using Firebase.Crashlytics;
using UnityEngine;

public class Logger {

	public static void d(string message) {
		Debug.Log(message);
        CrashlyticsLog(message);
	}

    public static void d(params object [] data)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = 0; i < data.Length; i++)
        {
            sb.Append(data[i].ToString());
            sb.Append(" ");
        }
        string message = sb.ToString();
        Debug.Log(message);

        CrashlyticsLog(message);
    }

	public static void d(System.DateTime date) {
		Debug.Log(date);
	}

	public static void e(string message) {
		Debug.LogError(message);
        CrashlyticsLog(message);
	}

    public static void e(params object [] data)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = 0; i < data.Length; i++)
        {
            sb.Append(data[i].ToString());
            sb.Append(" ");
        }
        string message = sb.ToString();
        Debug.LogError(message);

        CrashlyticsLog(message);
    }

    public static void w(string message) {
		Debug.LogWarning(message);
        CrashlyticsLog(message);
	}

    public static void w(params object [] data)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = 0; i < data.Length; i++)
        {
            sb.Append(data[i].ToString());
            sb.Append(" ");
        }
        string message = sb.ToString();
        Debug.LogWarning(message);

        CrashlyticsLog(message);
    }

    private static void CrashlyticsLog(string message)
    {
        
    }

	public static void d(string title, List<int> listInt) {
        System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
		stringBuilder.Append(title).Append(": ");
        for (int i = 0; i < listInt.Count; i++) {
			stringBuilder.Append(listInt[i]).Append((i == listInt.Count - 1) ? "" : " - ");
		}

		d(stringBuilder.ToString());
	}

	public static void d(string title, int[] arrayInt) {
        System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
		stringBuilder.Append(title).Append(": ");
        for (int i = 0; i < arrayInt.Length; i++) {
			stringBuilder.Append(arrayInt[i]);
			if (i < arrayInt.Length - 1)
				stringBuilder.Append(" - ");
		}

		d(stringBuilder.ToString());
	}
}
