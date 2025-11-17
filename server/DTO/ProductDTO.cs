namespace DTO
{
    public record ProductDto(
         int ProductId,
         string Name,
         int CategoryId,
         string Emoji,
         string Color
     );

    public record AddProductDto(
        string Name,
        int CategoryId,
        string Emoji
    );
}





