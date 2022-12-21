SET sqlformat csv
SET feedback off

SELECT DISTINCT spriden_last_name AS "Last Name",
                spriden_first_name AS "First Name",
                SUBSTR (spriden_mi, 1, 1) AS "Middle Name",
                /*gobtpac_external_user AS "USER NAME",*/
                'D'|| spriden_id AS "USER NAME",
                spriden_id AS "School ID Number",
                ... AS "Password",
                NVL (grad_date.grad_year, '2012') AS "Year of Graduation",
                goremal_email_address AS "Email Address"
           FROM spriden,
                goremal,
                ssbsect,
                sfrstcr,
                gobtpac,
                (SELECT   MAX (sgbstdn_term_code_eff), sgbstdn_pidm,
                          TO_CHAR (MAX (sgbstdn_exp_grad_date),
                                   'YYYY') AS grad_year
                     FROM sgbstdn
                    WHERE sgbstdn_exp_grad_date IS NOT NULL
                 GROUP BY sgbstdn_pidm) grad_date
          WHERE sfrstcr_pidm = spriden_pidm
            AND (    (ssbsect_term_code = sfrstcr_term_code)
                 AND (ssbsect_crn = sfrstcr_crn))
            AND sfrstcr_rsts_code LIKE 'R%'
            AND spriden_pidm = goremal_pidm(+)
            AND spriden_change_ind IS NULL
            AND sfrstcr_term_code = f_get_term(sysdate, 'pterm')
            AND (goremal_emal_code IS NULL OR goremal_emal_code = 'STU')
            AND spriden_pidm = grad_date.sgbstdn_pidm(+)
            AND spriden_pidm = gobtpac_pidm(+);

EXIT
