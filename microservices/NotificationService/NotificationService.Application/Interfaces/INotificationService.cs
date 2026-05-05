using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NotificationService.Domain.Entities;

namespace NotificationService.Application.Interfaces
{
    public interface INotificationService
    {
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(Guid userId);
        Task<bool> MarkAsReadAsync(Guid notificationId);
        Task<bool> MarkAllAsReadAsync(Guid userId);
        Task CreateNotificationAsync(Notification notification);
        Task<int> GetUnreadCountAsync(Guid userId);
    }
}
