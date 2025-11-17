namespace DTO
{
    public record CategoryDto(int CategoryId,string Name,string Emoji,string Color);
    public record AddCategoryDto(string Name,string Emoji);

}

