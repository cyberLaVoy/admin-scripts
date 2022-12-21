whenever sqlerror exit sql.sqlcode;

-- all non CE class section restrictions at once for all terms that haven't reached their ending date for Web registration
BEGIN 
    -- for current and every future term
    FOR term IN (SELECT sorrtrm_term_code FROM sorrtrm WHERE sorrtrm_end_date >= sysdate AND sorrtrm_term_code NOT LIKE '%30')
    LOOP

            -- for every non CE class (Success Academy not considered), where there is no entry in ssrrprg for class section
            FOR class IN ( SELECT ssbsect_crn
                           FROM ssbsect 
                           WHERE ssbsect_term_code = term.sorrtrm_term_code
                           AND NOT regexp_like(ssbsect_seq_numb, '..[V,J,X,Q]') -- V is on-campus concurrent enrollment, J and X is CE at highschool 
                           AND ssbsect_ssts_code = 'A' 
                           AND NOT EXISTS (SELECT * FROM ssrrprg 
                                        WHERE ssrrprg_term_code = term.sorrtrm_term_code 
                                        AND ssrrprg_crn = ssbsect_crn 
                                        AND ssrrprg_rec_type = 1) )
            LOOP
                BEGIN
                -- insert an Exclude base restriction
                INSERT INTO ssrrprg (ssrrprg_term_code, ssrrprg_crn, ssrrprg_rec_type, ssrrprg_activity_date, ssrrprg_program_ind) -- ssrrprg_surrogate_id, ssrrprg_version inserted by saturn.st_ssrrprg_surrogate_id trigger
                VALUES (term.sorrtrm_term_code, class.ssbsect_crn, 1, sysdate, 'E');
                END;
            END LOOP;

            -- for every non CE class (Success Academy not considered), where there is no ND-CONC program restriction or any Include restriction
            FOR class IN ( SELECT ssbsect_crn
                           FROM ssbsect 
                           WHERE ssbsect_term_code = term.sorrtrm_term_code
                           AND NOT regexp_like(ssbsect_seq_numb, '..[V,J,X,Q]') -- V is on-campus concurrent enrollment, J and X is CE at highschool 
                           AND ssbsect_ssts_code = 'A' 
                           AND NOT EXISTS (SELECT * FROM ssrrprg 
                                           WHERE ( ssrrprg_term_code = term.sorrtrm_term_code AND ssrrprg_crn = ssbsect_crn )
                                           AND ( ssrrprg_program = 'ND-CONC' OR ssrrprg_program_ind = 'I' )) )
            LOOP
                BEGIN
                -- interst ND-CONC restriction
                INSERT INTO ssrrprg (ssrrprg_term_code, ssrrprg_crn, ssrrprg_rec_type, ssrrprg_activity_date, ssrrprg_program) -- ssrrprg_surrogate_id, ssrrprg_version inserted by saturn.st_ssrrprg_surrogate_id trigger
                VALUES (term.sorrtrm_term_code, class.ssbsect_crn, 2, sysdate, 'ND-CONC');
                END;
            END LOOP;


            -- for every CE class, where there is no entry in ssrrprg for class section
            FOR class IN ( SELECT ssbsect_crn 
                           FROM ssbsect 
                           WHERE ssbsect_term_code = term.sorrtrm_term_code
                           AND regexp_like(ssbsect_seq_numb, '..[V,J,X,Q]') -- V is on-campus concurrent enrollment, J and X is CE at highschool 
                           AND ssbsect_seq_numb not like '_S_' -- S is success academy
                           AND ssbsect_ssts_code = 'A' 
                           AND NOT EXISTS (SELECT * FROM ssrrprg 
                                        WHERE ssrrprg_term_code = term.sorrtrm_term_code 
                                        AND ssrrprg_crn = ssbsect_crn 
                                        AND ssrrprg_rec_type = 1) )
            LOOP
                BEGIN
                -- insert an Include base restriction
                INSERT INTO ssrrprg (ssrrprg_term_code, ssrrprg_crn, ssrrprg_rec_type, ssrrprg_activity_date, ssrrprg_program_ind) -- ssrrprg_surrogate_id, ssrrprg_version inserted by saturn.st_ssrrprg_surrogate_id trigger
                VALUES (term.sorrtrm_term_code, class.ssbsect_crn, 1, sysdate, 'I');
                END;
            END LOOP;


            -- for every CE class, where there is no ND-CONC program restriction or any Exlcude restriction
            FOR class IN ( SELECT ssbsect_crn 
                           FROM ssbsect 
                           WHERE ssbsect_term_code = term.sorrtrm_term_code
                           AND regexp_like(ssbsect_seq_numb, '..[V,J,X,Q]') -- V is on-campus concurrent enrollment, J and X is CE at highschool 
                           AND ssbsect_seq_numb not like '_S_' -- S is success academy
                           AND ssbsect_ssts_code = 'A' 
                           AND NOT EXISTS (SELECT * FROM ssrrprg 
                                           WHERE ( ssrrprg_term_code = term.sorrtrm_term_code AND ssrrprg_crn = ssbsect_crn )
                                           AND ( ssrrprg_program = 'ND-CONC' OR ssrrprg_program_ind = 'E' )) )
            LOOP
                BEGIN
                -- interst ND-CONC restriction
                INSERT INTO ssrrprg (ssrrprg_term_code, ssrrprg_crn, ssrrprg_rec_type, ssrrprg_activity_date, ssrrprg_program) -- ssrrprg_surrogate_id, ssrrprg_version inserted by saturn.st_ssrrprg_surrogate_id trigger
                VALUES (term.sorrtrm_term_code, class.ssbsect_crn, 2, sysdate, 'ND-CONC');
                END;
            END LOOP;

            -- UPDATES on records
            -- for every CE class, where there is an ND-CONC Exclude program restriction
            FOR class IN ( SELECT ssbsect_crn 
                           FROM ssbsect 
                           WHERE ssbsect_term_code = term.sorrtrm_term_code
                           AND regexp_like(ssbsect_seq_numb, '..[V,J,X,Q]') -- V is on-campus concurrent enrollment, J and X is CE at highschool 
                           AND ssbsect_seq_numb not like '_S_' -- S is success academy
                           AND ssbsect_ssts_code = 'A' 
                           AND EXISTS (SELECT * FROM ssrrprg 
                                       WHERE ssrrprg_term_code = term.sorrtrm_term_code 
                                       AND ssrrprg_crn = ssbsect_crn 
                                       AND ssrrprg_program = 'ND-CONC') 
                           AND EXISTS (SELECT * FROM ssrrprg 
                                       WHERE ssrrprg_term_code = term.sorrtrm_term_code 
                                       AND ssrrprg_crn = ssbsect_crn 
                                       AND ssrrprg_program_ind = 'E') )
            LOOP
                BEGIN
                    -- set program restriction to Include
                    UPDATE ssrrprg 
                    SET ssrrprg_activity_date = sysdate, ssrrprg_program_ind = 'I'
                    WHERE ssrrprg_term_code = term.sorrtrm_term_code
                    AND ssrrprg_crn = class.ssbsect_crn
                    AND ssrrprg_rec_type = 1;
                END;
            END LOOP;

            -- UPDATES on records
            -- for every non-CE class, where there is an ND-CONC Include program restriction
            FOR class IN ( SELECT ssbsect_crn 
                           FROM ssbsect 
                           WHERE ssbsect_term_code = term.sorrtrm_term_code
                           AND NOT regexp_like(ssbsect_seq_numb, '..[V,J,X,Q]') -- V is on-campus concurrent enrollment, J and X is CE at highschool 
                           AND ssbsect_ssts_code = 'A' 
                           AND EXISTS (SELECT * FROM ssrrprg 
                                       WHERE ssrrprg_term_code = term.sorrtrm_term_code 
                                       AND ssrrprg_crn = ssbsect_crn 
                                       AND ssrrprg_program = 'ND-CONC') 
                           AND EXISTS (SELECT * FROM ssrrprg 
                                       WHERE ssrrprg_term_code = term.sorrtrm_term_code 
                                       AND ssrrprg_crn = ssbsect_crn 
                                       AND ssrrprg_program_ind = 'I') )
            LOOP
                BEGIN
                    -- delete ND-CONC as Included program
                    DELETE FROM ssrrprg 
                    WHERE ssrrprg_term_code = term.sorrtrm_term_code
                    AND ssrrprg_crn = class.ssbsect_crn
                    AND ssrrprg_rec_type = 2;
                END;
            END LOOP; 

    END LOOP;

END;
/

commit;
exit;
