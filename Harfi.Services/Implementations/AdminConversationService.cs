using Harfi.DTOs.Chat;
using Harfi.Models.Entities;
using Harfi.Repositories.Interfaces;
using Harfi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Harfi.Services.Implementations;

public class AdminConversationService : IAdminConversationService
{
    private readonly IConversationRepository _convRepo;

    public AdminConversationService(IConversationRepository convRepo)
    {
        _convRepo = convRepo;
    }

    public async Task<IEnumerable<AdminConversationDto>> GetAllConversationsAsync(
        ConversationFilterDto filter)
    {
        var query = _convRepo.GetAllConversationsQueryIgnoreFilters();

        if (!string.IsNullOrEmpty(filter.CustomerName))
            query = query.Where(c =>
                c.Customer!.Name.Contains(filter.CustomerName));

        if (!string.IsNullOrEmpty(filter.CraftsmanName))
            query = query.Where(c =>
                c.Craftsman!.User.Name.Contains(filter.CraftsmanName));

        if (!string.IsNullOrEmpty(filter.ServiceType))
            query = query.Where(c =>
                c.Job.ServiceType.Contains(filter.ServiceType));

        if (filter.DateFrom.HasValue)
            query = query.Where(c => c.CreatedAt >= filter.DateFrom.Value);

        if (filter.DateTo.HasValue)
            query = query.Where(c => c.CreatedAt <= filter.DateTo.Value);

        var conversations = await query
            .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return conversations.Select(MapToListDto);
    }

    public async Task<AdminConversationDetailDto?> GetConversationWithMessagesAsync(
        int conversationId)
    {
        var conversation = await _convRepo.GetByIdWithMessagesAsync(conversationId);
        if (conversation == null) return null;

        var dto = MapToListDto(conversation);

        return new AdminConversationDetailDto
        {
            Id = dto.Id,
            JobId = dto.JobId,
            CustomerName = dto.CustomerName,
            CraftsmanName = dto.CraftsmanName,
            ServiceType = dto.ServiceType,
            MessageCount = dto.MessageCount,
            LastMessageAt = dto.LastMessageAt,
            CreatedAt = dto.CreatedAt,
            Messages = conversation.Messages
                .OrderBy(m => m.SentAt)
                .Select(MapMessageDto)
                .ToList()
        };
    }

    private static AdminConversationDto MapToListDto(Conversation c)
    {
        var msgCount = c.Messages?.Count ?? 0;
        var lastMsg = c.Messages?
            .OrderByDescending(m => m.SentAt)
            .FirstOrDefault();

        return new AdminConversationDto
        {
            Id = c.Id,
            JobId = c.JobId,
            CustomerName = c.Customer?.Name ?? string.Empty,
            CraftsmanName = c.Craftsman?.User?.Name ?? string.Empty,
            ServiceType = c.Job?.ServiceType ?? string.Empty,
            MessageCount = msgCount,
            LastMessageAt = lastMsg?.SentAt ?? c.LastMessageAt,
            CreatedAt = c.CreatedAt
        };
    }

    private static MessageDto MapMessageDto(Message m) => new()
    {
        Id = m.Id,
        ConversationId = m.ConversationId,
        SenderId = m.SenderId,
        SenderName = m.Sender?.Name ?? string.Empty,
        SenderAvatar = m.Sender?.ProfileImageUrl,
        Content = m.Content,
        MessageType = m.MessageType,
        IsRead = m.IsRead,
        SentAt = m.SentAt
    };
}
