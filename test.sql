-- Test SQL file with various issues
select * from users, orders where users.id = orders.user_id;

UPDATE users SET status = 'active';

DELETE FROM temp_table;

SELECT u.name, o.total 
FROM users u 
CROSS JOIN orders o
WHERE u.active = 1;

DROP TABLE old_data;

TRUNCATE TABLE logs;