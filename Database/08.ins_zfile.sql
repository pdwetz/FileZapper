use zapper
go

if object_id('ins_zfile') is null
begin
	declare @sql varchar(max)
	set @sql = 'create procedure dbo.ins_zfile as select null'
	exec (@sql)
end
go

alter procedure dbo.ins_zfile
(
	@zfile_id uniqueidentifier,
	@zfile_name varchar(255),
	@zfile_ext varchar(50),
	@zfile_dir varchar(255),
	@zfile_path varchar(260),
	@zfile_size bigint,
	@zfile_hash varchar(50) = null,
	@zfile_modified smalldatetime = null
)

as

merge into zfile as t
using (select
	Id = @zfile_id,
	Name = @zfile_name,
	Extension = @zfile_ext,
	Directory = @zfile_dir,
	FullPath = @zfile_path,
	Size = @zfile_size,
	ContentHash = @zfile_hash,
	FileModified = @zfile_modified)
as s
	(Id, Name, Extension, Directory, FullPath, Size, ContentHash, FileModified)
on t.FullPath = s.FullPath
when matched then
	update set
		Name = s.Name,
		Extension = s.Extension,
		Directory = s.Directory,
		Id = s.Id,
		Size = s.Size,
		ContentHash = s.ContentHash,
		FileModified = s.FileModified,
		Modified = SYSDATETIME()
when not matched then
	insert (FullPath, Id, Name, Extension, Directory, Size, FileModified, ContentHash, Added, Modified)
	values (FullPath, Id, Name, Extension, Directory, Size, FileModified, ContentHash, SYSDATETIME(), SYSDATETIME())
;
