   M S B A M L   `   `   `  ����L  HPedestrianTracker, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null5clr-namespace:PedestrianTrackerPedestrianTracker  5   "   (    "PedestrianTracker.TrajectoryWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   .myclr-namespace:PedestrianTracker     5   "   -   5          � Title$  Trajectory Data��5   	   $	��600��6!   $	��600��6.    � Icon$ icons/pedtrac.ico��5   	    � 	Resources 5      +   %PedestrianTracker.TrajectoryDbDataSet   trajectoryDbDataSet  trajectoriesViewSource �� Source �� Path �� Source��%_   &	        &	      7     5   
   6!   �� 5	   
    6>   �� $ trajectories�8   5
      .��5      &   System.Windows.Controls.DataGrid  -   5        $��trajectoriesDataGrid��5   �      AutoGenerateColumns .     EnableRowVirtualization . $	��548��6J      RowDetailsVisibilityMode$ VisibleWhenSelected=�6�   $	��569��6�   ��6W   �� # [ 	   Columns	 5   
   0  *System.Windows.Controls.DataGridTextColumn  5      -   5        skeletonColumn��,  &System.Windows.Controls.DataGridColumn
   Header$
 skeleton��5   [      Width5  /System.Windows.Controls.DataGridLengthConverter$ SizeToHeader 6m   1  +System.Windows.Controls.DataGridBoundColumn   Binding 69   �� $ skeleton�6m     5      -   5        start_timeColumn��$
 
start time��5   ;   $ 80 6�    6O   �� $ 
start_time� �� StringFormat$ {0:hh:mm:ss.fff}��6�     5      -   5        end_timeColumn��$
 end time��5   9   $ 80 6�    6K   �� $ end_time�$ {0:hh:mm:ss.fff}��6�     5      -   5        average_velocityColumn��$
 average velocity��5   k   $ SizeToHeader 6�    6A   �� $ average_velocity�6�     5       -   5        average_directionColumn��$
 average direction��5    m   $ SizeToHeader 6�    6B   �� $ average_direction�6�     5!      -   5        lengthColumn��$
 length��5!   W   $ SizeToHeader 6g    67   �� $ length�6g     5"      -	   5        
t_idColumn��$

 t id��5"   S      
IsReadOnly . $ SizeToHeader 6s    65   �� $
 t_id�6s   5$      5%      