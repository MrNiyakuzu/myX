using System.Reflection;

public class Post
{
    public int Id { get; set; }
    public int AuthorId { get; set; }
    public string Hashtag { get; set; } = "";
    public string Time { get; set; } = DateTime.Now.ToString();
    public int Views { get; set; } = 0;
    public string Text { get; set; }
}