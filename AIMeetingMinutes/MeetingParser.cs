using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AIMeetingMinutes
{
    public static class MeetingParser
    {
        // 이름 + 시간: "홍길동 09:30" 또는 "홍길동 09:30:00"
        private static readonly Regex WithTime =
            new(@"^(.+?)\s+(\d{1,2}:\d{2}(?::\d{2})?)$", RegexOptions.Compiled);

        // 이름만: 한글/영문/숫자로만 구성된 짧은 줄 (공백 없는 이름 한 단어)
        // 조건: 줄 전체가 1~10자, 특수문자 없음, 뒤에 콜론 없음
        private static readonly Regex NameOnly =
            new(@"^([가-힣a-zA-Z0-9]{1,10})$", RegexOptions.Compiled);

        public static List<MeetingEntry> Parse(string transcript)
        {
            var entries = new List<MeetingEntry>();
            var lines = transcript.Split('\n');

            MeetingEntry? current = null;
            var contentBuffer = new System.Text.StringBuilder();

            foreach (var rawLine in lines)
            {
                var line = rawLine.TrimEnd();

                var matchTime = WithTime.Match(line);
                var matchName = !matchTime.Success ? NameOnly.Match(line) : System.Text.RegularExpressions.Match.Empty;

                bool isSpeakerLine = matchTime.Success || matchName.Success;

                if (isSpeakerLine)
                {
                    // 이전 항목 저장
                    if (current != null)
                    {
                        current.Content = contentBuffer.ToString().Trim();
                        if (!string.IsNullOrWhiteSpace(current.Content))
                            entries.Add(current);
                    }

                    current = new MeetingEntry
                    {
                        Speaker = matchTime.Success
                            ? matchTime.Groups[1].Value.Trim()
                            : matchName.Groups[1].Value.Trim(),
                        Timestamp = matchTime.Success ? matchTime.Groups[2].Value : ""
                    };
                    contentBuffer.Clear();
                }
                else if (current != null)
                {
                    if (contentBuffer.Length > 0) contentBuffer.Append('\n');
                    contentBuffer.Append(line);
                }
            }

            // 마지막 항목 저장
            if (current != null)
            {
                current.Content = contentBuffer.ToString().Trim();
                if (!string.IsNullOrWhiteSpace(current.Content))
                    entries.Add(current);
            }

            return entries;
        }
    }
}
