SET sqlformat csv
SET feedback off

SELECT subj_code || ' ' || crse_number AS "CourseNumber",
       seq_number_key AS "CourseSection", 
       term_code_key AS "Period",
       'Yes' AS "Remove"
FROM as_catalog_schedule 
LEFT join ssrxlst ON ssrxlst_term_code = term_code_key AND ssrxlst_crn = crn_key
WHERE term_code_key = f_get_term(sysdate, 'pterm')
AND seq_number_key =  dsc.get_xlst_master_seq(term_code_key, ssrxlst_xlst_group, subj_code, crse_number, seq_number_key) 
AND subj_code not in ( 'CED', 'ICL')
AND actual_enrollment = 0;

EXIT
