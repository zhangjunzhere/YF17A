CoDeSys+!   �                   @        @   2.3.9.31    @/    @                             ��xS +    @                           dlwQ        �   @   q   C:\TWINCAT\PLC\LIB\STANDARD.LIB @                                                                                          CONCAT               STR1               ��              STR2               ��                 CONCAT                                         d�66  �   ����           CTD           M             ��           Variable for CD Edge Detection      CD            ��           Count Down on rising edge    LOAD            ��           Load Start Value    PV           ��           Start Value       Q            ��           Counter reached 0    CV           ��           Current Counter Value             d�66  �   ����           CTU           M             ��            Variable for CU Edge Detection       CU            ��       
    Count Up    RESET            ��           Reset Counter to 0    PV           ��           Counter Limit       Q            ��           Counter reached the Limit    CV           ��           Current Counter Value             d�66  �   ����           CTUD           MU             ��            Variable for CU Edge Detection    MD             ��            Variable for CD Edge Detection       CU            ��	       
    Count Up    CD            ��
           Count Down    RESET            ��           Reset Counter to Null    LOAD            ��           Load Start Value    PV           ��           Start Value / Counter Limit       QU            ��           Counter reached Limit    QD            ��           Counter reached Null    CV           ��           Current Counter Value             d�66  �   ����           DELETE               STR               ��              LEN           ��              POS           ��                 DELETE                                         d�66  �   ����           F_TRIG           M             ��
                 CLK            ��           Signal to detect       Q            ��           Edge detected             d�66  �   ����           FIND               STR1               ��              STR2               ��                 FIND                                     d�66  �   ����           INSERT               STR1               ��              STR2               ��              POS           ��                 INSERT                                         d�66  �   ����           LEFT               STR               ��              SIZE           ��                 LEFT                                         d�66  �   ����           LEN               STR               ��                 LEN                                     d�66  �   ����           MID               STR               ��              LEN           ��              POS           ��                 MID                                         d�66  �   ����           R_TRIG           M             ��
                 CLK            ��           Signal to detect       Q            ��           Edge detected             d�66  �   ����           REPLACE               STR1               ��              STR2               ��              L           ��              P           ��                 REPLACE                                         d�66  �   ����           RIGHT               STR               ��              SIZE           ��                 RIGHT                                         d�66  �   ����           RS               SET            ��              RESET1            ��                 Q1            ��
                       d�66  �   ����           SEMA           X             ��                 CLAIM            ��	              RELEASE            ��
                 BUSY            ��                       d�66  �   ����           SR               SET1            ��              RESET            ��                 Q1            ��	                       d�66  �   ����           TOF           M             ��           internal variable 	   StartTime            ��           internal variable       IN            ��       ?    starts timer with falling edge, resets timer with rising edge    PT           ��           time to pass, before Q is set       Q            ��	       2    is FALSE, PT seconds after IN had a falling edge    ET           ��
           elapsed time             d�66  �   ����           TON           M             ��           internal variable 	   StartTime            ��           internal variable       IN            ��       ?    starts timer with rising edge, resets timer with falling edge    PT           ��           time to pass, before Q is set       Q            ��	       0    is TRUE, PT seconds after IN had a rising edge    ET           ��
           elapsed time             d�66  �   ����           TP        	   StartTime            ��           internal variable       IN            ��       !    Trigger for Start of the Signal    PT           ��       '    The length of the High-Signal in 10ms       Q            ��	           The pulse    ET           ��
       &    The current phase of the High-Signal             d�66  �   ����    R    @                                                                                          MAIN           aTON                    TON                    bTON                    TON                    Flag                                             R^FS  @    ����            
 �       	      ( �      K   �     K   �     K        K   "                 /         +     ��localhost       �¿w   }�@     �       �     ��     |� �\�wp �w�����¿w>�8     �� }�@        �  }�@     �%%Tw� X%%\         X%%   `%%|� �    ��     h� �� ��  �|��|������|�� }�@        �� }�@     �� Tw� �� |� Tw� ��_������ ]"�     ,   ,                                                        K         @   R^FS  /*BECKCONFI3*/
        !��d @   @   �   �     3               
   Standard            	��xS                        VAR_GLOBAL
END_VAR
                                                                                  "                          Standard
         MAIN����           ���� R^FS                 $����                                            Standard dlwQ	dlwQ                                       	R^FS                        VAR_CONFIG
END_VAR
                                                                                   '              , B W ��           Global_Variables R^FS	�7vS                     ~+  VAR_GLOBAL
(**DB Address**)
	DB8 AT %MW2010:ARRAY[0..100] OF INT;
(*******�õ�������ֵ�����±���*******)

	CigMakingSpeed  AT %IW330:INT;
(**Packer Address Define**)
	PackingSpeed  AT %IW332:INT;
(*ManPotentiometerSpeed  Address*)
	PotenSpeed AT %IW328:INT;
(*****Level Address  Define********)
	ElevatorLevel AT %IW324:INT;
(*****Drop Address  Define********)
	DropLevel AT %IW326:INT;

	Store_set_zero AT %MX1000.0 : BOOL;
	test_run AT %MX1000.6 : BOOL;
	StoreUnit_discharge_button AT %MX2603.6 : BOOL;
	Elevater_man_paikong AT %MX2603.7 :BOOL;
	test_run_unprotected AT %MX1000.7 : BOOL;
	test_maker_speed AT %MW1950 : INT;
	test_packer_speed AT %MW1952 : INT;
	Corner_manual_speed AT %MW254 : INT;
	Store_manual_speed AT %MW260 : INT;
	DropTrans_cig_speed AT %MW2320 : INT;(*�½���������֧�ٶ�*)(*******7Axis**********)
	DropTrans_speed_rpm AT %MW2340 : INT;(*�½������͵��ת��*)(******7Axis***********)


	Lift_p_parameter AT %MW270 : INT;
	Transfer_p_parameter AT %MW272 : INT;
	MakerExport_p_parameter AT %MW274 : INT;



	Emergency_stop AT %IX0.0:BOOL;(*����ֹͣ�̵���*)
	MakerExit_power AT %IX0.1:BOOL;(*����*)
	Sample_power AT %IX0.2:BOOL;(*ȡ���ŷ�����Դ*)
	Corner_power AT %IX0.3:BOOL;(*����ŷ�����Դ*)
	Lift_power AT %IX0.4:BOOL;(*�����ŷ�����Դ*)
	Transfer_power AT %IX0.5:BOOL;(*�����ŷ�����Դ*)
	Slope_power AT %IX0.6:BOOL;(*б���ŷ�����Դ*)
	Store_power AT %IX0.7:BOOL;(*�洢�ŷ�����Դ*)
	DropTrans_power AT %IX1.0:BOOL;(*�½��������ŷ�����Դ*)(****************************************************7Axis����DropTrans_power�滻ԭSpare10*************)

	Spare11 AT %IX1.1:BOOL;(*����*)
	MakerExit_servo_fault AT %IX1.2:BOOL;(*����*)
	Sample_servo_fault AT %IX1.3:BOOL;(*ȡ���ŷ�����������*)
	Corner_servo_fault AT %IX1.4:BOOL;(*����ŷ�����������*)
	Lift_servo_fault AT %IX1.5:BOOL;(*�����ŷ�����������*)
	Transfer_servo_fault AT %IX1.6:BOOL;(*�����ŷ�����������*)
	Slope_servo_fault AT %IX1.7:BOOL;(*б���ŷ�����������*)
	Store_servo_fault AT %IX2.0:BOOL;(*�洢�ŷ�����������*)
	DropTrans_servo_fault AT %IX2.1:BOOL;(*����*)(********************************************************************7Axis����DropTrans_servo_fault�滻ԭSpare21*************)
	Spare22 AT %IX2.2:BOOL;(*����*)
	Elevater_man_auto_sw AT %IX2.3:BOOL;(*����*)
	Elevater_start_pb AT %IX2.4:BOOL;(*����*)
	DropTransLowLevel AT %IX2.5:BOOL;(*�½������͵���λ*)(********************************************************7Axis����DropTransLowLevel�滻ԭElevater_reset_pb*************)
	DropTransHighLevel AT %IX2.6:BOOL;(*�½������͸���λ*)(******************************************************7Axis����DropTransHighLevel�滻ԭElevater_stop_pb*************)
	DropTransLimitLevel AT %IX2.7:BOOL;(*�½������ͼ�����λ*)(*****************************************************7Axis����DropTransLimitLevel�滻ԭElevater_e_stop*************)
	Spare30 AT %IX3.0:BOOL;(*����*)
	Spare31 AT %IX3.1:BOOL;(*����*)
	MakerExit_sensor AT %IX3.2:BOOL;(*���̻���������(����)*)
	Sample_entrance_sensor AT %IX3.3:BOOL;(*ȡ��������̴�����*)
	Sample_entrance_jam_sensor AT %IX3.4:BOOL;(*ȡ����ڶ��������������ã�*)
	Corner_entrance_jam_sensor AT %IX3.5:BOOL;(*�����ڶ���������*)
	MakerExit_jam_sensor AT %IX3.6:BOOL;(*���̻����ڶ��������ã�*)
	Spare37 AT %IX3.7:BOOL;(*����*)
	Downport_jam_sensor AT %IX4.0:BOOL;(*�½��ڶ���������*)
	Slope_empty AT %IX4.1:BOOL;(*б��ͨ����*)
	Transfer_cig_exist AT %IX4.2:BOOL;(*�߼���֧������*)
	Transfer_overload_sensor AT %IX4.3:BOOL;(*���͹��ش�����*)
	Spare44 AT %IX4.4:BOOL;(*����*)
	Spare45 AT %IX4.5:BOOL;(*����*)
	StoreUnit_e_stop_button :BOOL;
	Spare46 AT %IX4.6:BOOL;(*����*)
	Spare47 AT %IX4.7:BOOL;(*����*)
	Store_full AT %IX5.0:BOOL;(*�洢����������*)
	Store_empty AT %IX5.1:BOOL;(*�洢���մ�����*)
	Store_overload AT %IX5.2:BOOL;(*�洢�����ش�����*)
	Store_entrance_cig_exist AT %IX5.3:BOOL;(*�洢��������̴�����*)
	Store_entrance_jam AT %IX5.4:BOOL;(*�洢����ڶ���*)
	Store_overlimit AT %IX5.5:BOOL;(*�洢�����޿���*)
	Store_running AT %IX5.6:BOOL;(*�洢��������*)
	Store_enabled AT %IX5.7:BOOL;(*�洢��ʹ��*)
	sample_servo_enable_Q AT %QX4.0:BOOL;(*ȡ���ŷ�������ʹ��*)
	Corner_servo_enable_Q AT %QX4.1:BOOL;(*����ŷ�������ʹ��*)
	Lift_servo_enable_Q AT %QX4.2:BOOL;(*�����ŷ�������ʹ��*)
	Transfer_servo_enable_Q AT %QX4.3:BOOL;(*�����ŷ�������ʹ��*)
	Slope_servo_enable_Q AT %QX4.4:BOOL;(*б���ŷ�������ʹ��*)
	Store_servo_enable_Q AT %QX4.5:BOOL;(*�洢�ŷ�������ʹ��*)
	DropTrans_servo_enable_Q AT %QX4.6:BOOL;(*�½��������ŷ�������ʹ��*)(*******************************7Axis����DropTrans_servo_enable_Q�滻ԭSpareOutput46*************)
	SpareOutput47 AT %QX4.7:BOOL;(*����*)
	Elevater_start_Q AT %QX5.0:BOOL;(*����*)
	Elevater_reset_Q AT %QX5.1:BOOL;(*����*)
	Elevater_stop_Q AT %QX5.2:BOOL;(*����*)
	Store_FaultReset_Q AT %QX5.3:BOOL;(*�洢�����ϸ�λ*)
	Maker_enable_relay_Q AT %QX5.4:BOOL;(*���̻�����(����)*)
	SpareOutput55 AT %QX5.5:BOOL;(*�½��ڼ�ͣ��λ*)
	SpareOutput56 AT %QX5.6:BOOL;(*����*)
	Maker_QuickStop_Q AT %QX5.7:BOOL;(*���̻���ͣ�����ã�*)
	SpareOutput80 AT %QX8.0:BOOL;(*�洢�񶯼��*)
	SpareOutput81 AT %QX8.1:BOOL;(*����*)
	Packer_enable_relay_Q AT %QX8.2:BOOL;(*��װ�����������ã�*)
	Packer_LowSpeed_request_Q AT %QX8.3:BOOL;(*��װ���������󣨱��ã�*)
	SpareOutput84 AT %QX8.4:BOOL;(*����*)
	SpareOutput85 AT %QX8.5:BOOL;(*ϵͳ����Դ*)
	SpareOutput86 AT %QX8.6:BOOL;(*����*)
	StoreUnit_start_Q AT %QX8.7:BOOL;(*����*)
	StoreUnit_reset_Q AT %QX9.0:BOOL;(*����*)
	StoreUnit_stop_Q AT %QX9.1:BOOL;(*����*)
	SpareOutput92 AT %QX9.2:BOOL;(*����*)
	SpareOutput93 AT %QX9.3:BOOL;(*����*)
	SpareOutput94 AT %QX9.4:BOOL;(*����*)
	SpareOutput95 AT %QX9.5:BOOL;(*����*)
	SpareOutput96 AT %QX9.6:BOOL;(*����*)
	SpareOutput97 AT %QX9.7:BOOL;(*����*)


	DropTrans_servo_enable AT %MX3102.5:BOOL;(*�½�������������ʹ��*)(*******************7Axis��*********************************)
	DropTrans_servo_initialized AT %MX3102.7:BOOL;(*�½������ͳ�ʼ���*)(*******************7Axis��*********************************)

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

	alarm_droptrans_jam AT %MX920.8 : BOOL;(*�½������Ͷ���*)(*******************7Axis��*********************************)



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
	DropTrans_servo_ethercat_fault AT %MX2600.7 : BOOL;(*�½��������ŷ�ͨѶ������*)(*************************7Axis����DropTrans_servo_ethercat_fault�滻ԭSpare26007*************)
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
	Sample_Servo_FaultNum AT %MW2804 : DWORD;
	Corner_Servo_FaultNum AT %MW2808 : DWORD;
	Lift_Servo_FaultNum AT %MW2812 : DWORD;

	Transfer_Servo_FaultNum AT %MW2816 : DWORD;
	Slope_Servo_FaultNum AT %MW2820 : DWORD;
	Store_Servo_FaultNum AT %MW2824 : DWORD;
	DropTrans_Servo_FaultNum AT %MW2828 : DWORD;(*�½��������ŷ����������ϴ���*)(***********7Axis�����½������͵�����ϴ���**************)




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
                                                                                               '           	   , , < ��           Variable_Configuration R^FS	R^FS	                        VAR_CONFIG
END_VAR
                                                                                                 �   |0|0 @|    @Z   MS Sans Serif @       HH':'mm':'ss @      dd'-'MM'-'yyyy   dd'-'MM'-'yyyy HH':'mm':'ss�����                               4     �   ���  �3 ���   � ���     
    @��  ���     @      DEFAULT             System      �   |0|0 @|    @Z   MS Sans Serif @       HH':'mm':'ss @      dd'-'MM'-'yyyy   dd'-'MM'-'yyyy HH':'mm':'ss�����                      )   HH':'mm':'ss @                             dd'-'MM'-'yyyy @       '         , , : ��           MAIN �moS	R^FS                      J   PROGRAM MAIN
VAR
	aTON: TON;
	bTON: TON;
	Flag: BOOL := TRUE;
END_VAR1	  (*aTON(IN:=bTON.Q , PT:=T#200ms , Q=> , ET=> );
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

(*	WriteFlag:=0;*)
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



                 ����                   "   STANDARD.LIB 5.6.98 11:03:02 @�_w5      CONCAT @                	   CTD @        	   CTU @        
   CTUD @           DELETE @           F_TRIG @        
   FIND @           INSERT @        
   LEFT @        	   LEN @        	   MID @           R_TRIG @           REPLACE @           RIGHT @           RS @        
   SEMA @           SR @        	   TOF @        	   TON @           TP @              Global Variables 0 @                                             2                ����������������  
             ����  
ntIN
        ����                                POUs                MAIN      ����          
   Data types  ����             Visualizations  ����              Global Variables                Global_Variables                     Variable_Configuration  	   ����                                                              dlwQ                         	   localhost            P      	   localhost            P      	   localhost            P            O�kX