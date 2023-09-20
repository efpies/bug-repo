using QoodenTask.Enums;

namespace QoodenTask.Models;

public class Migration
{
    public int Id { get; set; }
    public string SourceName { get; set; }
    public string SourcePath { get; set; }
    public MigrationSourceType SourceType { get; set; }
    public MigrationStatus Status { get; set; }
}