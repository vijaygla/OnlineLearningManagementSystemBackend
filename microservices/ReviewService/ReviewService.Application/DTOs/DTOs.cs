namespace ReviewService.Application.DTOs;

public record ReviewDto(Guid Id, Guid StudentId, Guid CourseId, int Rating, string Comment, DateTime CreatedAt);
public record CreateReviewRequest(Guid CourseId, int Rating, string Comment);
public record UpdateReviewRequest(int Rating, string Comment);
public record CourseRatingResponse(Guid CourseId, double AverageRating, int TotalReviews);
