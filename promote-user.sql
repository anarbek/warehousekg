INSERT INTO ""AspNetUserRoles"" (""UserId"", ""RoleId"")
SELECT u.""Id"", r.""Id""
FROM ""AspNetUsers"" u, ""AspNetRoles"" r
WHERE u.""Email"" = 'anarbek@gmail.com' AND r.""Name"" = 'Manager'
AND NOT EXISTS (
  SELECT 1 FROM ""AspNetUserRoles"" ur
  WHERE ur.""UserId"" = u.""Id"" AND ur.""RoleId"" = r.""Id""
);
