﻿using Avalonia.Controls.Notifications;

namespace YMCL.Public.Classes.Data;

public record NotificationEntry(object Content, NotificationType Type)
{
    public object Content { get; set; } = Content;
    public NotificationType Type { get; set; } = Type;


    public virtual bool Equals(NotificationEntry? other)
    {
        return Content == other?.Content;
    }
}