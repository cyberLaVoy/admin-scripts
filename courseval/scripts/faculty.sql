SET sqlformat csv
SET feedback off

SELECT DISTINCT primary_instructor_last_name "Last Name",
                primary_instructor_first_name "First Name",
                primary_instructor_middle_init "Middle Initial",
                'D'|| primary_instructor_id "User Name", 
                primary_instructor_id "School ID Number",
                dept_desc "Department Name",
                'D'|| primary_instructor_id ||'@domain' "Email Address"
FROM as_catalog_schedule 
WHERE term_code_key = f_get_term(sysdate, 'pterm')
and primary_instructor_id is not null;

EXIT
