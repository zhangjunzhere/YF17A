CoDeSys+!                      @        @   2.3.9.31    @/    @                             WYS +    @                           dlwQ        ј   @   q   C:\TWINCAT\PLC\LIB\STANDARD.LIB @                                                                                          CONCAT               STR1               §џ              STR2               §џ                 CONCAT                                         d66     џџџџ           CTD           M             §џ           Variable for CD Edge Detection      CD            §џ           Count Down on rising edge    LOAD            §џ           Load Start Value    PV           §џ           Start Value       Q            §џ           Counter reached 0    CV           §џ           Current Counter Value             d66     џџџџ           CTU           M             §џ            Variable for CU Edge Detection       CU            §џ       
    Count Up    RESET            §џ           Reset Counter to 0    PV           §џ           Counter Limit       Q            §џ           Counter reached the Limit    CV           §џ           Current Counter Value             d66     џџџџ           CTUD           MU             §џ            Variable for CU Edge Detection    MD             §џ            Variable for CD Edge Detection       CU            §џ	       
    Count Up    CD            §џ
           Count Down    RESET            §џ           Reset Counter to Null    LOAD            §џ           Load Start Value    PV           §џ           Start Value / Counter Limit       QU            §џ           Counter reached Limit    QD            §џ           Counter reached Null    CV           §џ           Current Counter Value             d66     џџџџ           DELETE               STR               §џ              LEN           §џ              POS           §џ                 DELETE                                         d66     џџџџ           F_TRIG           M             §џ
                 CLK            §џ           Signal to detect       Q            §џ           Edge detected             d66     џџџџ           FIND               STR1               §џ              STR2               §џ                 FIND                                     d66     џџџџ           INSERT               STR1               §џ              STR2               §џ              POS           §џ                 INSERT                                         d66     џџџџ           LEFT               STR               §џ              SIZE           §џ                 LEFT                                         d66     џџџџ           LEN               STR               §џ                 LEN                                     d66     џџџџ           MID               STR               §џ              LEN           §џ              POS           §џ                 MID                                         d66     џџџџ           R_TRIG           M             §џ
                 CLK            §џ           Signal to detect       Q            §џ           Edge detected             d66     џџџџ           REPLACE               STR1               §џ              STR2               §џ              L           §џ              P           §џ                 REPLACE                                         d66     џџџџ           RIGHT               STR               §џ              SIZE           §џ                 RIGHT                                         d66     џџџџ           RS               SET            §џ              RESET1            §џ                 Q1            §џ
                       d66     џџџџ           SEMA           X             §џ                 CLAIM            §џ	              RELEASE            §џ
                 BUSY            §џ                       d66     џџџџ           SR               SET1            §џ              RESET            §џ                 Q1            §џ	                       d66     џџџџ           TOF           M             §џ           internal variable 	   StartTime            §џ           internal variable       IN            §џ       ?    starts timer with falling edge, resets timer with rising edge    PT           §џ           time to pass, before Q is set       Q            §џ	       2    is FALSE, PT seconds after IN had a falling edge    ET           §џ
           elapsed time             d66     џџџџ           TON           M             §џ           internal variable 	   StartTime            §џ           internal variable       IN            §џ       ?    starts timer with rising edge, resets timer with falling edge    PT           §џ           time to pass, before Q is set       Q            §џ	       0    is TRUE, PT seconds after IN had a rising edge    ET           §џ
           elapsed time             d66     џџџџ           TP        	   StartTime            §џ           internal variable       IN            §џ       !    Trigger for Start of the Signal    PT           §џ       '    The length of the High-Signal in 10ms       Q            §џ	           The pulse    ET           §џ
       &    The current phase of the High-Signal             d66     џџџџ    R    @                                                                                          MAIN           aTON                    TON                    bTON                    TON                    Flag                                             -ШНQ  @    џџџџ            
 З       	      ( у      K   ё     K   џ     K        K   "                 /         +     КЛlocalhost       уТПw   }ё@            А     мз     |й \Рwp ОwџџџџуТПw>8     мћ }ё@          }ё@     %%Twэ X%%\         X%%   `%%|й А    џџ     hж аи Ди  щ|Р|џџџџЛ|мћ }ё@        мћ }ё@     Јк Twэ Јк |й Twэ л_џџџџй ]"э     ,   ,                                                        K         @   -ШНQ  /*BECKCONFI3*/
        !РЈd @   @           3               
   Standard            	WYS                        VAR_GLOBAL
END_VAR
                                                                                  "                          Standard
         MAINџџџџ           џџџџ -ШНQ                 $ћџџџ                                            Standard dlwQ	dlwQ                                       	-ШНQ                        VAR_CONFIG
END_VAR
                                                                                   '              , B W Ђм           Global_Variables -ШНQ	WYS                     Ъ   VAR_GLOBAL

	DB8 AT %MW2010:ARRAY[0..100] OF INT;

	Store_set_zero AT %MX1000.0 : BOOL;
	test_run AT %MX1000.6 : BOOL;
	StoreUnit_discharge_button AT %MX2603.6 : BOOL;
	Elevater_man_paikong AT %MX2603.7 :BOOL;
	test_run_unprotected AT %MX1000.7 : BOOL;
	test_maker_speed AT %MW1950 : INT;
	test_packer_speed AT %MW1952 : INT;
	Corner_manual_speed AT %MW254 : INT;
	Store_manual_speed AT %MW260 : INT;
	Lift_p_parameter AT %MW270 : INT;
	Transfer_p_parameter AT %MW272 : INT;
	MakerExport_p_parameter AT %MW274 : INT;
	Emergency_stop AT %IX0.0 : BOOL;
	MakerExit_power AT %IX0.1 : BOOL;
	Sample_power AT %IX0.2 : BOOL;
	Corner_power AT %IX0.3 : BOOL;
	Lift_power AT %IX0.4 : BOOL;
	Transfer_power AT %IX0.5 : BOOL;
	Slope_power AT %IX0.6 : BOOL;
	Store_power AT %IX0.7 : BOOL;
	Spare10 AT %IX1.0 : BOOL;
	Spare11 AT %IX1.1 : BOOL;
	MakerExit_servo_fault AT %IX1.2 : BOOL;
	Sample_servo_fault AT %IX1.3 : BOOL;
	Corner_servo_fault AT %IX1.4 : BOOL;
	Lift_servo_fault AT %IX1.5 : BOOL;
	Transfer_servo_fault AT %IX1.6 : BOOL;
	Slope_servo_fault AT %IX1.7 : BOOL;
	Store_servo_fault AT %IX2.0 : BOOL;
	Spare21 AT %IX2.1 : BOOL;
	Spare22 AT %IX2.2 : BOOL;
	Elevater_man_auto_sw AT %IX2.3 : BOOL;
	Elevater_start_pb AT %IX2.4 : BOOL;
	Elevater_reset_pb AT %IX2.5 : BOOL;
	Elevater_stop_pb AT %IX2.6 : BOOL;
	Elevater_e_stop AT %IX2.7 : BOOL;
	Spare30 AT %IX3.0 : BOOL;
	Spare31 AT %IX3.1 : BOOL;
	MakerExit_sensor AT %IX3.2 : BOOL;
	Sample_entrance_sensor AT %IX3.3 : BOOL;
	Sample_entrance_jam_sensor AT %IX3.4 : BOOL;
	Corner_entrance_jam_sensor AT %IX3.5 : BOOL;
	MakerExit_jam_sensor AT %IX3.6 : BOOL;
	Spare37 AT %IX3.7 : BOOL;
	StoreUnit_man_auto_sw AT %IX4.0 : BOOL;
	Spare41 AT %IX4.1 : BOOL;
	StoreUnit_start_button AT %IX4.2 : BOOL;
	StoreUnit_reset_button AT %IX4.3 : BOOL;
	StoreUnit_stop_button AT %IX4.4 : BOOL;
	StoreUnit_e_stop_button AT %IX4.5 : BOOL;
	Spare46 AT %IX4.6 : BOOL;
	Spare47 AT %IX4.7 : BOOL;
	Downport_jam_sensor AT %IX5.0 : BOOL;
	Slope_empty AT %IX5.1 : BOOL;
	Transfer_cig_exist AT %IX5.2 : BOOL;
	Transfer_overload_sensor AT %IX5.3 : BOOL;
	Spare54 AT %IX5.4 : BOOL;
	Spare55 AT %IX5.5 : BOOL;
	Spare56 AT %IX5.6 : BOOL;
	Spare57 AT %IX5.7 : BOOL;
	Store_full AT %IX6.0 : BOOL;
	Store_empty AT %IX6.1 : BOOL;
	Store_overload AT %IX6.2 : BOOL;
	Store_entrance_cig_exist AT %IX6.3 : BOOL;
	Store_entrance_jam AT %IX6.4 : BOOL;
	Store_overlimit AT %IX6.5 : BOOL;
	Store_running AT %IX6.6 : BOOL;
	Store_enabled AT %IX6.7 : BOOL;
	sample_servo_enable_Q AT %QX4.0 : BOOL;
	Corner_servo_enable_Q AT %QX4.1 : BOOL;
	Lift_servo_enable_Q AT %QX4.2 : BOOL;
	Transfer_servo_enable_Q AT %QX4.3 : BOOL;
	Slope_servo_enable_Q AT %QX4.4 : BOOL;
	Store_servo_enable_Q AT %QX4.5 : BOOL;
	SpareOutput46 AT %QX4.6 : BOOL;
	SpareOutput47 AT %QX4.7 : BOOL;
	Elevater_start_Q AT %QX5.0 : BOOL;
	Elevater_reset_Q AT %QX5.1 : BOOL;
	Elevater_stop_Q AT %QX5.2 : BOOL;
	Elevater_FaultReset_Q AT %QX5.3 : BOOL;
	Maker_enable_relay_Q AT %QX5.4 : BOOL;
	SpareOutput55 AT %QX5.5 : BOOL;
	SpareOutput56 AT %QX5.6 : BOOL;
	Maker_QuickStop_Q AT %QX5.7 : BOOL;
	SpareOutput80 AT %QX8.0 : BOOL;
	SpareOutput81 AT %QX8.1 : BOOL;
	Packer_enable_relay_Q AT %QX8.2 : BOOL;
	Packer_LowSpeed_request_Q AT %QX8.3 : BOOL;
	SpareOutput84 AT %QX8.4 : BOOL;
	SpareOutput85 AT %QX8.5 : BOOL;
	SpareOutput86 AT %QX8.6 : BOOL;
	StoreUnit_start_Q AT %QX8.7 : BOOL;
	StoreUnit_reset_Q AT %QX9.0 : BOOL;
	StoreUnit_stop_Q AT %QX9.1 : BOOL;
	SpareOutput92 AT %QX9.2 : BOOL;
	SpareOutput93 AT %QX9.3 : BOOL;
	SpareOutput94 AT %QX9.4 : BOOL;
	SpareOutput95 AT %QX9.5 : BOOL;
	SpareOutput96 AT %QX9.6 : BOOL;
	SpareOutput97 AT %QX9.7 : BOOL;
	MakerExit_servo_enable AT %MX2102.5 : BOOL;
	MakerExit_servo_initialized AT %MX2102.7 : BOOL;
	sample_servo_enable AT %MX1502.5 : BOOL;
	sample_servo_initialized AT %MX1502.7 : BOOL;
	Corner_servo_enable AT %MX1602.5 : BOOL;
	Corner_servo_initialized AT %MX1602.7 : BOOL;
	Slope_servo_enable AT %MX1702.5 : BOOL;
	Slope_servo_initialized AT %MX1702.7 : BOOL;
	Store_servo_enable AT %MX1802.5 : BOOL;
	Lift_servo_enable AT %MX1902.5 : BOOL;
	Lift_servo_initialized AT %MX1902.7 : BOOL;
	Transfer_servo_enable AT %MX2002.5 : BOOL;
	Transfer_servo_initialized AT %MX2002.7 : BOOL;
	alarm_sample_entrance_jam AT %MX50.0 : BOOL;
	alarm_corner_entrance_jam AT %MX50.1 : BOOL;
	alarm_downport_entrance_jam AT %MX50.2 : BOOL;
	alarm_transfer_overload AT %MX50.3 : BOOL;
	alarm_store_overload AT %MX50.4 : BOOL;
	alarm_store_entrance_jam AT %MX50.5 : BOOL;
	Elevater_manual_discharge AT %MX501.0 : BOOL;
	Elevater_auto_run AT %MX502.1 : BOOL;
	StoreUnit_man_run AT %MX550.1 : BOOL;
	StoreUnit_auto_run AT %MX552.1 : BOOL;
	StoreUnit_discharge_run AT %MX553.7 : BOOL;
	alarm_store_limit_on AT %MX60.2 : BOOL;
	alarm_store_full AT %MX71.2 : BOOL;
	alarm_encoder_fault AT %MX71.3 : BOOL;
	Maker_run AT %MX80.0 : BOOL;
	packer_run AT %MX80.1 : BOOL;
	MakerExit_servo_ethercat_fault AT %MX2600.0 : BOOL;
	Sample_servo_ethercat_fault AT %MX2600.1 : BOOL;
	Corner_servo_ethercat_fault AT %MX2600.2 : BOOL;
	Lift_servo_ethercat_fault AT %MX2600.3 : BOOL;
	Transfer_servo_ethercat_fault AT %MX2600.4 : BOOL;
	Slope_servo_ethercat_fault AT %MX2600.5 : BOOL;
	Store_servo_ethercat_fault AT %MX2600.6 : BOOL;
	Spare26007 AT %MX2600.7 : BOOL;
	Spare26010 AT %MX2601.0 : BOOL;
	Digital_Input1_ethercat_fault AT %MX2601.1 : BOOL;
	Digital_Input2_ethercat_fault AT %MX2601.2 : BOOL;
	Digital_Input3_ethercat_fault AT %MX2601.3 : BOOL;
	Digital_Input4_ethercat_fault AT %MX2601.4 : BOOL;
	Digital_Input5_ethercat_fault AT %MX2601.5 : BOOL;
	Digital_Input6_ethercat_fault AT %MX2601.6 : BOOL;
	Digital_Input7_ethercat_fault AT %MX2601.7 : BOOL;
	Digital_Output1_ethercat_fault AT %MX2602.0 : BOOL;
	Digital_Output2_ethercat_fault AT %MX2602.1 : BOOL;
	Digital_Output3_ethercat_fault AT %MX2602.2 : BOOL;
	Digital_Output4_ethercat_fault AT %MX2602.3 : BOOL;
	Analog_Input1_ethercat_fault AT %MX2602.4 : BOOL;
	Analog_Input2_ethercat_fault AT %MX2602.5 : BOOL;
	StoreUnit_Stop AT %MX2603.0 : BOOL;
	Elevater_Stop AT %MX2603.1 : BOOL;
	Manual_Run AT %MX2603.2 : BOOL;
	Auto_Run AT %MX2603.3 : BOOL;
	Elevater_Manual_Run AT %MX2603.4 : BOOL;
	test_run_light AT %MX1002.0 : BOOL;
	Downport_comp_output AT %MW1250 : INT;
	Maker_cig_speed AT %MW1300 : INT;
	Sample_cig_speed AT %MW1302 : INT;
	Corner_cig_speed AT %MW1304 : INT;
	Packer_cig_speed AT %MW1310 : INT;
	Life_cig_speed AT %MW1400 : INT;
	Transfer_cig_speed AT %MW1402 : INT;
	MakerExport_cig_speed AT %MW1404 : INT;
	MakerExport_speed_rpm AT %MW2236 : INT;
	Sample_speed_rpm AT %MW1536 : INT;
	Corner_speed_rpm AT %MW1636 : INT;
	Slope_speed_rpm AT %MW1736 : INT;
	Store_speed_rpm AT %MW1738 : INT;
	Corner_entrance_sensor_output AT %MW180 : INT;
	Downport_sensor_output AT %MW182 : INT;
	Store_CigNum AT %MW1880 : INT;
         Store_CigNum2 AT %MW1882 : INT;
	Store_cig_speed AT %MW1884 : INT;
	Slope_cig_speed AT %MW1886 : INT;
	Lift_speed_rpm AT %MW1936 : INT;
	Transfer_speed_rpm AT %MW2036 : INT;
	MakerExport_Servo_FaultNum AT %MW2800 : DWORD;
	Sample_Servo_FaultNum AT %MW2802 : DWORD;
	Corner_Servo_FaultNum AT %MW2804 : DWORD;
	Lift_Servo_FaultNum AT %MW2806 : DWORD;

	Transfer_Servo_FaultNum AT %MW2808 : DWORD;
	Slope_Servo_FaultNum AT %MW2810 : DWORD;
	Store_Servo_FaultNum AT %MW2812 : DWORD;





	Corner_pid_sp:INT;
	Corner_work_limit:INT;
	Corner_work_off_delay:INT;
	Corner_pid_p_gain:INT;
	Corner_p_parameter:INT;
	cig_dim:INT;
	Store_CigIn_Comp_speed1:INT;
	Store_CigIn_Comp_speed2:INT;
	Downport_CigIn_hight2:INT;
	Corner_pid_i_gain:INT;
	Maker_MaxSpeedLimit:INT;
	Packer_MaxSpeedLimit:INT;
	Store_empty_position:INT;
	Store_full_position:INT;
	Packer_LowSpeed_position:INT;
	Packer_enable_position:INT;
	Maker_stop_position:INT;
	Corner_entrance_hight_limit:INT;
	Corner_entrance_low_limt:INT;
	Downport_CigIn_hight1:INT;
	Corner_pid_deadband:INT;
	Downport_CigOut_hight1:INT;
	Downport_CigOut_hight2:INT;
	Store_CigOut_Comp_speed1:INT;
	Store_CigOut_Comp_speed2:INT;
	Downport_Highest_limit:INT;
	Downport_Lowest_limit:INT;
	Downport_CigIn_lowest_hight:INT;
	Corner_pid_output:INT;
	Corner_pid_pv:INT;
	DownPort_hight:INT;
	Store_percent:INT;
	Corner_lowlimit:INT;


	WriteFlag:BOOL;

END_VAR
                                                                                               '           	   , , < жф           Variable_Configuration -ШНQ	-ШНQ	                        VAR_CONFIG
END_VAR
                                                                                                    |0|0 @|    @Z   MS Sans Serif @       HH':'mm':'ss @      dd'-'MM'-'yyyy   dd'-'MM'-'yyyy HH':'mm':'ssѓџџџ                               4     џ   џџџ  Ь3 џџџ   џ џџџ     
    @џ  џџџ     @      DEFAULT             System         |0|0 @|    @Z   MS Sans Serif @       HH':'mm':'ss @      dd'-'MM'-'yyyy   dd'-'MM'-'yyyy HH':'mm':'ssѓџџџ                      )   HH':'mm':'ss @                             dd'-'MM'-'yyyy @       '         , , : П           MAIN gb%S	-ШНQ                      J   PROGRAM MAIN
VAR
	aTON: TON;
	bTON: TON;
	Flag: BOOL := TRUE;
END_VAR*	  (*aTON(IN:=bTON.Q , PT:=T#200ms , Q=> , ET=> );
bTON(IN:= NOT aTON.Q , PT:=T#200ms , Q=> , ET=> );
Man_Auto_OP2:=bTON.Q;
*)

;

IF WriteFlag THEN

	DB8[0]:=Corner_pid_sp;
	DB8[36]:=Corner_work_limit;
	DB8[38]:=Corner_work_off_delay;
	DB8[4]:=Corner_pid_p_gain;
	DB8[40]:=Corner_p_parameter;
	DB8[46]:=cig_dim;
	DB8[52]:=Store_CigIn_Comp_speed1;
	DB8[54]:=Store_CigIn_Comp_speed2;
	DB8[56]:=Downport_CigIn_hight2;
	DB8[6]:=Corner_pid_i_gain;
	DB8[60]:=Maker_MaxSpeedLimit;
	DB8[62]:=Packer_MaxSpeedLimit;
	DB8[64]:=Store_empty_position;
	DB8[66]:=Store_full_position;
	DB8[68]:=Packer_LowSpeed_position;
	DB8[70]:=Packer_enable_position;
	DB8[72]:=Maker_stop_position;
	DB8[74]:=Corner_entrance_hight_limit;
	DB8[76]:=Corner_entrance_low_limt;
	DB8[78]:=Downport_CigIn_hight1;
	DB8[8]:=Corner_pid_deadband;
	DB8[80]:=Downport_CigOut_hight1;
	DB8[82]:=Downport_CigOut_hight2;
	DB8[84]:=Store_CigOut_Comp_speed1;
	DB8[86]:=Store_CigOut_Comp_speed2;
	DB8[88]:=Downport_Highest_limit;
	DB8[90]:=Downport_Lowest_limit;
	DB8[92]:=Downport_CigIn_lowest_hight;
	
	DB8[10]:=Corner_pid_output;
	DB8[2]:=Corner_pid_pv;
	DB8[26]:=DownPort_hight;
	DB8[50]:=Store_percent;

	DB8[58]:=Corner_lowlimit;

	WriteFlag:=0;
ELSE


	Corner_pid_sp:=DB8[0];
	Corner_work_limit:=DB8[36];
	Corner_work_off_delay:=DB8[38];
	Corner_pid_p_gain:=DB8[4];
	Corner_p_parameter:=DB8[40];
	cig_dim:=DB8[46];
	Store_CigIn_Comp_speed1:=DB8[52];
	Store_CigIn_Comp_speed2:=DB8[54];
	Downport_CigIn_hight2:=DB8[56];
	Corner_pid_i_gain:=DB8[6];
	Maker_MaxSpeedLimit:=DB8[60];
	Packer_MaxSpeedLimit:=DB8[62];
	Store_empty_position:=DB8[64];
	Store_full_position:=DB8[66];
	Packer_LowSpeed_position:=DB8[68];
	Packer_enable_position:=DB8[70];
	Maker_stop_position:=DB8[72];
	Corner_entrance_hight_limit:=DB8[74];
	Corner_entrance_low_limt:=DB8[76];
	Downport_CigIn_hight1:=DB8[78];
	Corner_pid_deadband:=DB8[8];
	Downport_CigOut_hight1:=DB8[80];
	Downport_CigOut_hight2:=DB8[82];
	Store_CigOut_Comp_speed1:=DB8[84];
	Store_CigOut_Comp_speed2:=DB8[86];
	Downport_Highest_limit:=DB8[88];
	Downport_Lowest_limit:=DB8[90];
	Downport_CigIn_lowest_hight:=DB8[92];
	
	Corner_pid_output:=DB8[10];
	Corner_pid_pv:=DB8[2];
	DownPort_hight:=DB8[26];
	Store_percent:=DB8[50];

	Corner_lowlimit:=DB8[58];

END_IF


                 §џџџ                   "   STANDARD.LIB 5.6.98 11:03:02 @ц_w5      CONCAT @                	   CTD @        	   CTU @        
   CTUD @           DELETE @           F_TRIG @        
   FIND @           INSERT @        
   LEFT @        	   LEN @        	   MID @           R_TRIG @           REPLACE @           RIGHT @           RS @        
   SEMA @           SR @        	   TOF @        	   TON @           TP @              Global Variables 0 @                                             2                џџџџџџџџџџџџџџџџ  
             њџџџ  
ntIN
        јџџџ                                POUs                MAIN      џџџџ           
   Data types  џџџџ              Visualizations  џџџџ              Global Variables                Global_Variables                     Variable_Configuration  	   џџџџ                                                              dlwQ                         	   localhost            P      	   localhost            P      	   localhost            P           9SЧ